using System.Xml.Linq;

namespace Zadatak2;

internal class Models
{
}

public class MobilePhone
{
    public int OwnerId { get; set; }
    public string Manufacturer { get; set; }
    public string Model { get; set; }
    public string Network { get; set; }
    public string OS { get; set; }
}

public class Owner
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
}

public class SoldPhonesDB
{
    public List<MobilePhone> SoldPhones { get; set; }
}

public interface ISoldPhonesDBUpgrade
{
    List<MobilePhone> Get4GHuaweiPhones();
}

public class UpgradedSoldPhonesDB : SoldPhonesDB, ISoldPhonesDBUpgrade
{
    public List<MobilePhone> Get4GHuaweiPhones()
    {
        var query = from phone in SoldPhones
            where phone.Network == "4G" && phone.Manufacturer == "Huawei"
            select phone;
        return query.ToList();
    }
}

public class OwnersDB
{
    public List<Owner> Owners { get; set; }
}

public interface IOwnersDBUpgrade
{
    List<Owner> GetPrimeAgedOwners();
}

public class UpgradedOwnersDB : OwnersDB, IOwnersDBUpgrade
{
    public List<Owner> GetPrimeAgedOwners()
    {
        return (from owner in Owners
            where owner.Age is >= 20 and <= 40
            select owner).ToList();
    }
}

public static class MobilePhoneShop
{
    public static UpgradedSoldPhonesDB SPDB;
    public static UpgradedOwnersDB ODB;

    static MobilePhoneShop()
    {
        var xmlData = XElement.Load("../../../Data.xml");
        FetchSoldPhones(xmlData);
        FetchOwners(xmlData);
    }

    private static void FetchOwners(XElement xmlData)
    {
        ODB = new UpgradedOwnersDB();
        var fetchedOwners = from owner in xmlData.Descendants("Owner")
            select new Owner
            {
                Id = (int)owner.Attribute("Id"),
                Name = (string)owner,
                Age = (int)owner.Attribute("Age")
            };
        ODB.Owners = fetchedOwners.ToList();
    }

    private static void FetchSoldPhones(XElement xmlData)
    {
        SPDB = new UpgradedSoldPhonesDB
        {
            SoldPhones = (from phone in xmlData.Descendants("Phone")
                select new MobilePhone
                {
                    OwnerId = (int)phone.Attribute("OwnerId"),
                    Manufacturer = (string)phone.Attribute("Manufacturer"),
                    Model = (string)phone,
                    Network = (string)phone.Attribute("Network"),
                    OS = (string)phone.Attribute("OS")
                }).ToList()
        };
    }

    public static void AndroidOwnersCountReport()
    {
        var query = from phone in SPDB.SoldPhones
            where phone.OS == "Android"
            group phone by phone.OwnerId
            into ownerGroup
            join owner in ODB.Owners on ownerGroup.Key equals owner.Id
            orderby ownerGroup.Count() descending
            select new
            {
                OwnerName = owner.Name,
                AndroidPhoneCount = ownerGroup.Count()
            };

        query.ToList().ForEach(x =>
            Console.WriteLine($"Owner: {x.OwnerName}, Android phone count: {x.AndroidPhoneCount}"));
    }


    public static void Report4GHuaweiNonPrimeAge()
    {
        var nonPrimeAgedOwners = ODB.Owners.Where(x => x.Age is < 20 or > 40).ToList();
        var report = from owner in nonPrimeAgedOwners
            join phone in SPDB.Get4GHuaweiPhones() on owner.Id equals phone.OwnerId
                into ownerGroup
            where ownerGroup.Any()
            select new
            {
                OwnerName = owner.Name,
                Phones = ownerGroup
            };

        foreach (var x in report)
        {
            Console.WriteLine($"Owner: {x.OwnerName}");
            x.Phones.ToList().ForEach(phone =>
                Console.WriteLine($"\t - {phone.Model}"));
        }
    }

    public static void Report8CharacterName()
    {
        var report = from owner in ODB.GetPrimeAgedOwners()
            where owner.Name.Length < 8
            join phone in SPDB.SoldPhones on owner.Id equals phone.OwnerId
            where phone.Manufacturer == "Xiaomi" && phone.Network == "4G"
            group phone by owner
            into ownerGroup
            where ownerGroup.Count() < 2
            select new
            {
                OwnerName = ownerGroup.Key.Name
            };

        report.ToList().ForEach(x => Console.WriteLine($"Owner: {x.OwnerName}"));
    }
}