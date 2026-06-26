using UnityEngine;

public static class DocumentGenerator
{
    static readonly string[] merchantNames = {
        "Edmund of Ashford", "Gilbert of Thornwick", "Thomas the Miller",
        "Robert of Crestholm", "Walter of Dunmere", "Hugh of Aldgate",
        "William the Cooper", "Richard of Fenwick"
    };

    static readonly string[] cargoTypes = {
        "Grain", "Wheat", "Barley", "Rye",
        "Apples", "Pears", "Dried Figs",
        "Pepper", "Cinnamon", "Salt", "Saffron"
    };

    static readonly string[] villages = {
        "Thornwick", "Ashford", "Crestholm", "Dunmere",
        "Aldgate", "Fenwick", "Millhaven", "Ironford",
        "Greymoor", "Saltbury", "Coldwell"
    };

    static readonly string[] fakeMerchantNames = {
        "Edmond of Ashford", "Gilbert of Thornwik", "Thomas the Miler",
        "Robert of Crestholme", "Walter of Dunmeer", "Hugh of Aldgaet",
        "William the Cuoper", "Richard of Fenwikk"
    };

    public static readonly int TodayDay = 15;
    public static readonly int TodayMonth = 10;
    public static readonly int TodayYear = 1347;

    public static string TodayDate => FormatDate(0);

    public static DocumentData Generate(bool makeFake)
    {
        DocumentData doc = new DocumentData();

        int nameIdx = Random.Range(0, merchantNames.Length);
        int cargoIdx = Random.Range(0, cargoTypes.Length);
        int villageIdx = Random.Range(0, villages.Length);

        doc.merchantName = merchantNames[nameIdx];
        doc.cargoType = cargoTypes[cargoIdx];
        doc.originVillage = villages[villageIdx];
        doc.sealRequired = true;
        doc.maxDaysOld = 7;

        doc.docMerchantName = doc.merchantName;
        doc.docCargoType = doc.cargoType;
        doc.docOriginVillage = doc.originVillage;
        doc.docHasSeal = true;
        doc.docDaysOld = Random.Range(0, doc.maxDaysOld + 1);

        doc.isValid = true;
        doc.invalidField = "";

        if (makeFake)
        {
            int flaw = Random.Range(0, 5);

            switch (flaw)
            {
                case 0:
                    doc.docMerchantName = fakeMerchantNames[nameIdx];
                    doc.invalidField = "name";
                    break;

                case 1:
                    string wrongCargo = cargoTypes[Random.Range(0, cargoTypes.Length)];
                    while (wrongCargo == doc.cargoType)
                        wrongCargo = cargoTypes[Random.Range(0, cargoTypes.Length)];

                    doc.docCargoType = wrongCargo;
                    doc.invalidField = "cargo";
                    break;

                case 2:
                    string wrongVillage = villages[Random.Range(0, villages.Length)];
                    while (wrongVillage == doc.originVillage)
                        wrongVillage = villages[Random.Range(0, villages.Length)];

                    doc.docOriginVillage = wrongVillage;
                    doc.invalidField = "origin";
                    break;

                case 3:
                    doc.docHasSeal = false;
                    doc.invalidField = "seal";
                    break;

                case 4:
                    doc.docDaysOld = Random.Range(doc.maxDaysOld + 1, doc.maxDaysOld + 15);
                    doc.invalidField = "date";
                    break;
            }

            doc.isValid = false;
        }

        return doc;
    }

    public static string FormatDate(int daysOld)
    {
        string[] months = {
            "January","February","March","April","May","June",
            "July","August","September","October","November","December"
        };

        int day = TodayDay - daysOld;
        int month = TodayMonth;
        int year = TodayYear;

        while (day <= 0)
        {
            month--;

            if (month <= 0)
            {
                month = 12;
                year--;
            }

            day += DaysInMonth(month);
        }

        return $"{Ordinal(day)} of {months[month - 1]}, {year}";
    }

    static int DaysInMonth(int month)
    {
        int[] days = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
        return days[month - 1];
    }

    static string Ordinal(int n)
    {
        if (n % 100 >= 11 && n % 100 <= 13)
            return n + "th";

        return (n % 10) switch
        {
            1 => n + "st",
            2 => n + "nd",
            3 => n + "rd",
            _ => n + "th"
        };
    }
}