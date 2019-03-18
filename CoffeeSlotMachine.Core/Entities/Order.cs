using System;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace CoffeeSlotMachine.Core.Entities
{
    /// <summary>
    /// Bestellung verwaltet das bestellte Produkt, die eingeworfenen Münzen und
    /// die Münzen die zurückgegeben werden.
    /// </summary>
    public class Order : EntityObject
    {

        /// <summary>
        /// Datum und Uhrzeit der Bestellung
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Werte der eingeworfenen Münzen als Text. Die einzelnen 
        /// Münzwerte sind durch ; getrennt (z.B. "10;20;10;50")
        /// </summary>
        public String ThrownInCoinValues { get; set; }

        /// <summary>
        /// Zurückgegebene Münzwerte mit ; getrennt
        /// </summary>
        public String ReturnCoinValues { get; set; }

        /// <summary>
        /// Summe der eingeworfenen Cents.
        /// </summary>
        public int ThrownInCents => string.IsNullOrEmpty(ThrownInCoinValues)?0:ThrownInCoinValues.Split(';').Select(s => int.Parse(s)).Sum();

        /// <summary>
        /// Summe der Cents die zurückgegeben werden
        /// </summary>
        public int ReturnCents => ReturnCoinValues.Split(';').Select(s => int.Parse(s)).Sum();


        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }

        /// <summary>
        /// Kann der Automat mangels Kleingeld nicht
        /// mehr herausgeben, wird der Rest als Spende verbucht
        /// </summary>
        public int DonationCents => ThrownInCents - Product.PriceInCents - ReturnCents;

        /// <summary>
        /// Münze wird eingenommen.
        /// </summary>
        /// <param name="coinValue"></param>
        /// <returns>isFinished ist true, wenn der Produktpreis zumindest erreicht wurde</returns>
        public bool InsertCoin(int coinValue)
        {
            if (coinValue == 0) return false;
            if (ThrownInCents < Product.PriceInCents)
            { 
                 AddIntToNumbersText(coinValue);
            }
            return ThrownInCents >= Product.PriceInCents;
        }

        /// <summary>
        /// Übernahme des Einwurfs in das Münzdepot.
        /// Rückgabe des Retourgeldes aus der Kasse. Staffelung des Retourgeldes
        /// hängt vom Inhalt der Kasse ab.
        /// </summary>
        /// <param name="coins">Aktueller Zustand des Münzdepots</param>
        public void FinishPayment(IEnumerable<Coin> coins)
        {
            var insertedCoins = ThrownInCoinValues.Split(';')
                .GroupBy(coin => coin)
                .Select(co => new
                {
                    CoinValue = int.Parse(co.Key),
                    Amount = co.Count()
                }).OrderBy(co => co.CoinValue);

            foreach (var item in insertedCoins)
            {
                coins.Single(coin => coin.CoinValue == item.CoinValue)
                    .Amount += item.Amount;
            }
            // retourGeld
            int returnCents = ThrownInCents - Product.PriceInCents;
            if (returnCents == 0) ReturnCoinValues = "0";
            else
            {
                foreach (var coin in coins)
                {
                    while (returnCents > 0 && coin.CoinValue <= returnCents && coin.Amount > 0)
                    {
                        if (!string.IsNullOrEmpty(ReturnCoinValues)) ReturnCoinValues = $"{ReturnCoinValues};{coin.CoinValue}";
                        else ReturnCoinValues = $"{coin.CoinValue}";
                        returnCents -= coin.CoinValue;
                        coin.Amount--;
                    }
                }
            }
        }


        public void AddIntToNumbersText(int coinValue)
        {
            if(string.IsNullOrEmpty(ThrownInCoinValues))
            {
                ThrownInCoinValues += $"{coinValue}";
            }
            else
            {
                ThrownInCoinValues += $";{coinValue}";
            }
        }

    }
}
