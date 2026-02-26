using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

namespace LufthansaTetris
{
    struct Csomag
    {
        public string Azonosito;
        public double Suly;
        public double Terfogat;

        public Csomag(string azonosito, double suly, double terfogat)
        {
            Azonosito = azonosito;
            Suly = suly;
            Terfogat = terfogat;
        }
    }

    struct CsaladiCsomagok
    {
        public string CsaladAzonosito;
        public double OsszSuly;
        public double OsszTerfogat;
        public List<Csomag> Csomagok;

        public CsaladiCsomagok(string id)
        {
            CsaladAzonosito = id;
            OsszSuly = 0;
            OsszTerfogat = 0;
            Csomagok = new List<Csomag>();
        }
    }

    struct Kontener
    {
        public int Azonosito;
        public int Erokar;
        public double JelenlegiSuly;
        public double JelenlegiTerfogat;
        public List<CsaladiCsomagok> BetoltottCsaladiCsomagok;

        public Kontener(int azonosito, int erokar)
        {
            Azonosito = azonosito;
            Erokar = erokar;
            JelenlegiSuly = 0;
            JelenlegiTerfogat = 0;
            BetoltottCsaladiCsomagok = new List<CsaladiCsomagok>();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, CsaladiCsomagok> csaladok = BeolvasAdatok();
            Kontener[] kontenerok = InicializalKontenerek();

            MohoAlgoritmus(csaladok, kontenerok);
            EredmenyKiirasa(kontenerok);

            Console.ReadKey();
        }

        static Dictionary<string, CsaladiCsomagok> BeolvasAdatok()
        {
            Dictionary<string, CsaladiCsomagok> adatok = new Dictionary<string, CsaladiCsomagok>();
            string fajlnev = "../../csomagok.csv";

            StreamReader sr = new StreamReader(fajlnev);

            while (!sr.EndOfStream)
            {
                string sor = sr.ReadLine();

                if (sor != null && sor.Length > 0)
                {
                    string[] elemek = sor.Split(';');
                    if (elemek.Length >= 4)
                    {
                        string csomagId = elemek[0];
                        string csaladId = elemek[1];
                        double suly = double.Parse(elemek[2], CultureInfo.InvariantCulture);
                        double terfogat = double.Parse(elemek[3], CultureInfo.InvariantCulture);

                        Csomag ujCsomag = new Csomag(csomagId, suly, terfogat);

                        if (!adatok.ContainsKey(csaladId))
                        {
                            adatok.Add(csaladId, new CsaladiCsomagok(csaladId));
                        }

                        CsaladiCsomagok aktualisCsalad = adatok[csaladId];
                        aktualisCsalad.Csomagok.Add(ujCsomag);
                        aktualisCsalad.OsszSuly += suly;
                        aktualisCsalad.OsszTerfogat += terfogat;
                        adatok[csaladId] = aktualisCsalad;
                    }
                }
            }

            sr.Close();

            return adatok;
        }

        static Kontener[] InicializalKontenerek()
        {
            Kontener[] kontenerek = new Kontener[5];
            kontenerek[0] = new Kontener(1, -2);
            kontenerek[1] = new Kontener(2, -1);
            kontenerek[2] = new Kontener(3, 0);
            kontenerek[3] = new Kontener(4, 1);
            kontenerek[4] = new Kontener(5, 2);
            return kontenerek;
        }

        static bool BeleferE(Kontener k, CsaladiCsomagok cs)
        {
            double maxSuly = 1500.0;
            double maxTerfogat = 6.0;

            bool eredmeny = true;

            if (k.JelenlegiSuly + cs.OsszSuly > maxSuly)
            {
                eredmeny = false;
            }

            if (eredmeny)
            {
                if (k.JelenlegiTerfogat + cs.OsszTerfogat > maxTerfogat)
                {
                    eredmeny = false;
                }
            }

            return eredmeny;
        }

        static double SzamitCG(Kontener[] kontenerek)
        {
            double nyomatekOsszeg = 0;
            double osszSuly = 0;

            for (int i = 0; i < kontenerek.Length; i++)
            {
                nyomatekOsszeg += kontenerek[i].JelenlegiSuly * kontenerek[i].Erokar;
                osszSuly += kontenerek[i].JelenlegiSuly;
            }

            double eredmeny = 0;
            if (osszSuly != 0)
            {
                eredmeny = nyomatekOsszeg / osszSuly;
            }
            return eredmeny;
        }

        static void MohoAlgoritmus(Dictionary<string, CsaladiCsomagok> csaladok, Kontener[] kontenerek)
        {
            foreach (string kulcs in csaladok.Keys)
            {
                CsaladiCsomagok aktualisCsalad = csaladok[kulcs];
                int legjobbKontenerIndex = -1;
                double legjobbCGElteres = double.MaxValue;

                for (int i = 0; i < kontenerek.Length; i++)
                {
                    if (BeleferE(kontenerek[i], aktualisCsalad))
                    {
                        double eredetiSuly = kontenerek[i].JelenlegiSuly;
                        kontenerek[i].JelenlegiSuly += aktualisCsalad.OsszSuly;

                        double ujCG = SzamitCG(kontenerek);
                        double elteres = Math.Abs(ujCG);

                        if (elteres < legjobbCGElteres)
                        {
                            legjobbCGElteres = elteres;
                            legjobbKontenerIndex = i;
                        }

                        kontenerek[i].JelenlegiSuly = eredetiSuly;
                    }
                }

                if (legjobbKontenerIndex != -1)
                {
                    kontenerek[legjobbKontenerIndex].BetoltottCsaladiCsomagok.Add(aktualisCsalad);
                    kontenerek[legjobbKontenerIndex].JelenlegiSuly += aktualisCsalad.OsszSuly;
                    kontenerek[legjobbKontenerIndex].JelenlegiTerfogat += aktualisCsalad.OsszTerfogat;
                }
            }
        }

        static void EredmenyKiirasa(Kontener[] kontenerek)
        {
            for (int i = 0; i < kontenerek.Length; i++)
            {
                Console.WriteLine($"Konténer {kontenerek[i].Azonosito} (Erőkar: {kontenerek[i].Erokar}): " +
                                  $"{kontenerek[i].JelenlegiSuly:F2} kg / {kontenerek[i].JelenlegiTerfogat:F2} m^3 - " +
                                  $"{kontenerek[i].BetoltottCsaladiCsomagok.Count} család");
            }

            double vegsoCG = SzamitCG(kontenerek);
            Console.WriteLine();
            Console.WriteLine($"A repülőgép VÉGSŐ súlypontja: {vegsoCG:F4}");

            if (Math.Abs(vegsoCG) < 0.5)
            {
                Console.WriteLine("A gép tökéletes egyensúlyban van.");
            }
            else
            {
                Console.WriteLine("A gép nincs egyensúlyban.");
            }
        }
    }
}