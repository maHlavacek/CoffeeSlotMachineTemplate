using CoffeeSlotMachine.Core.Logic;
using CoffeeSlotMachine.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace CoffeeSlotMachine.ControllerTest
{
    [TestClass]
    public class ControllerTest
    {
        [TestInitialize]
        public void MyTestInitialize()
        {
            using (ApplicationDbContext applicationDbContext = new ApplicationDbContext())
            {
                applicationDbContext.Database.EnsureDeleted();
                applicationDbContext.Database.Migrate();
            }
        }


        [TestMethod]
        public void T01_GetCoinDepot_CoinTypesCount_ShouldReturn6Types_3perType_SumIs1155Cents()
        {
            using (OrderController controller = new OrderController())
            {
                var depot = controller.GetCoinDepot().ToArray();
                Assert.AreEqual(6, depot.Count(), "Sechs Münzarten im Depot");
                foreach (var coin in depot)
                {
                    Assert.AreEqual(3, coin.Amount, "Je Münzart sind drei Stück im Depot");
                }

                int sumOfCents = depot.Sum(coin => coin.CoinValue * coin.Amount);
                Assert.AreEqual(1155, sumOfCents, "Beim Start sind 1155 Cents im Depot");
            }
        }

        [TestMethod]
        public void T02_GetProducts_9Products_FromCappuccinoToRistretto()
        {
            using (OrderController statisticsController = new OrderController())
            {
                var products = statisticsController.GetProducts().ToArray();
                Assert.AreEqual(9, products.Length, "Neun Produkte wurden erzeugt");
                Assert.AreEqual("Cappuccino", products[0].Name);
                Assert.AreEqual("Ristretto", products[8].Name);
            }
        }

        [TestMethod]
        public void T03_BuyOneCoffee_OneCoinIsEnough_CheckCoinsAndOrders()
        {
            using (OrderController controller = new OrderController())
            {
                var products = controller.GetProducts();
                var product = products.Single(p => p.Name == "Cappuccino");
                var order = controller.OrderCoffee(product);
                bool isFinished = controller.InsertCoin(order, 100);
                Assert.AreEqual(true, isFinished, "100 Cent genügen");
                Assert.AreEqual(100, order.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(100 - product.PriceInCents, order.ReturnCents);
                Assert.AreEqual(0, order.DonationCents);
                Assert.AreEqual("20;10;5", order.ReturnCoinValues);

                // Depot überprüfen
                var coins = controller.GetCoinDepot().ToArray();
                int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
                Assert.AreEqual(1220, sumOfCents, "Beim Start sind 1155 Cents + 65 Cents für Cappuccino");
                Assert.AreEqual("3*200 + 4*100 + 3*50 + 2*20 + 2*10 + 2*5", controller.GetCoinDepotString());

                var orders = controller.GetAllOrdersWithProduct().ToArray();
                Assert.AreEqual(1, orders.Length, "Es ist genau eine Bestellung");
                Assert.AreEqual(0, orders[0].DonationCents, "Keine Spende");
                Assert.AreEqual(100, orders[0].ThrownInCents, "100 Cents wurden eingeworfen");
                Assert.AreEqual("Cappuccino", orders[0].Product.Name, "Produktname Cappuccino");
            }
        }

        [TestMethod]
        public void T04_BuyOneCoffee_ExactThrowInOneCoin_CheckCoinsAndOrders()
        {
            using (OrderController controller = new OrderController())
            {
                var products = controller.GetProducts();
                var product = products.Single(p => p.Name == "Latte");
                var order = controller.OrderCoffee(product);
                bool isFinished = controller.InsertCoin(order, 50);
                Assert.AreEqual(true, isFinished, "50 Cent genügen");
                Assert.AreEqual(50, order.ThrownInCents, "Einwurf stimmt");
                Assert.AreEqual(50 - product.PriceInCents, order.ReturnCents);
                Assert.AreEqual(0, order.DonationCents);
                Assert.AreEqual("0", order.ReturnCoinValues);

                // Depot überprüfen
                var coins = controller.GetCoinDepot().ToArray();
                int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
                Assert.AreEqual(1205, sumOfCents, "Beim Start sind 1155 Cents + 50 Cents für Latte");
                Assert.AreEqual("3*200 + 3*100 + 4*50 + 3*20 + 3*10 + 3*5", controller.GetCoinDepotString());

                var orders = controller.GetAllOrdersWithProduct().ToArray();
                Assert.AreEqual(1, orders.Length, "Es ist genau eine Bestellung");
                Assert.AreEqual(0, orders[0].DonationCents, "Keine Spende");
                Assert.AreEqual(50, orders[0].ThrownInCents, "50 Cents wurden eingeworfen");
                Assert.AreEqual("Latte", orders[0].Product.Name, "Produktname Latte");
            }
        }

        [TestMethod]
        public void T05_BuyOneCoffee_MoreCoins_CheckCoinsAndOrders()
        {
            using (OrderController controller = new OrderController())
            {
                var products = controller.GetProducts();
                var product = products.Single(p => p.Name == "Latte");
                var order = controller.OrderCoffee(product);
                bool isFinished = controller.InsertCoin(order, 10);
                Assert.AreEqual(false, isFinished, "10 Cent genügen nicht");
                isFinished = controller.InsertCoin(order, 20);
                Assert.AreEqual(false, isFinished, "30 Cent genügen nicht");
                isFinished = controller.InsertCoin(order, 20);
                Assert.AreEqual(true, isFinished, "50 Cent genügen");
                Assert.AreEqual(50, order.ThrownInCents, "Einwurf stimmt");
                Assert.AreEqual(50 - product.PriceInCents, order.ReturnCents);
                Assert.AreEqual(0, order.DonationCents);
                Assert.AreEqual("0", order.ReturnCoinValues);

                // Depot überprüfen
                var coins = controller.GetCoinDepot().ToArray();
                int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
                Assert.AreEqual(1205, sumOfCents, "Beim Start sind 1155 Cents + 50 Cents für Latte");
                Assert.AreEqual("3*200 + 3*100 + 3*50 + 5*20 + 4*10 + 3*5", controller.GetCoinDepotString());

                var orders = controller.GetAllOrdersWithProduct().ToArray();
                Assert.AreEqual(1, orders.Length, "Es ist genau eine Bestellung");
                Assert.AreEqual(0, orders[0].DonationCents, "Keine Spende");
                Assert.AreEqual(50, orders[0].ThrownInCents, "50 Cents wurden eingeworfen");
                Assert.AreEqual("Latte", orders[0].Product.Name, "Produktname Latte");
            }
        }


        [TestMethod()]
        public void T06_BuyMoreCoffees_OneCoins_CheckCoinsAndOrders()
        {
            using (OrderController controller = new OrderController())
            {
                //Latte 1
                var products = controller.GetProducts();
                var product = products.Single(p => p.Name == "Latte");
                var order = controller.OrderCoffee(product);
                bool isFinished = controller.InsertCoin(order, 100);
                Assert.AreEqual(true, isFinished, "100 Cent genügen");
                Assert.AreEqual(100, order.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(100 - product.PriceInCents, order.ReturnCents);
                Assert.AreEqual(0, order.DonationCents);
                Assert.AreEqual("50", order.ReturnCoinValues);
                int returnCoin = int.Parse(order.ReturnCoinValues);

                //Latte2
                products = controller.GetProducts();
                product = products.Single(p => p.Name == "Latte");
                order = controller.OrderCoffee(product);
                isFinished = controller.InsertCoin(order, returnCoin);
                Assert.AreEqual(true, isFinished, "50 Cent genügen");
                Assert.AreEqual(returnCoin, order.ThrownInCents, "Einwurf stimmt");
                Assert.AreEqual(returnCoin - product.PriceInCents, order.ReturnCents);
                Assert.AreEqual(0, order.DonationCents);
                Assert.AreEqual("0", order.ReturnCoinValues);

                // Depot überprüfen
                var coins = controller.GetCoinDepot().ToArray();
                int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
                Assert.AreEqual(1255, sumOfCents, "Beim Start sind 1155 Cents + 100 Cents für Latte");
                Assert.AreEqual("3*200 + 4*100 + 3*50 + 3*20 + 3*10 + 3*5", controller.GetCoinDepotString());

                var orders = controller.GetAllOrdersWithProduct().ToArray();
                Assert.AreEqual(2, orders.Length, "Es sind genau zwei Bestellungen");
                Assert.AreEqual(0, orders[0].DonationCents, "Keine Spende");
                Assert.AreEqual(0, orders[1].DonationCents, "Keine Spende");
                Assert.AreEqual(100, orders[0].ThrownInCents, "100 Cents wurden eingeworfen");
                Assert.AreEqual(50, orders[1].ThrownInCents, "50 Cents wurden eingeworfen");
                Assert.AreEqual("Latte", orders[0].Product.Name, "Produktname Latte");
                Assert.AreEqual("Latte", orders[1].Product.Name, "Produktname Latte");
            }
        }


        [TestMethod()]
        public void T07_BuyMoreCoffees_UntilDonation_CheckCoinsAndOrders()
        {
            using (OrderController controller = new OrderController())
            {
                //Latte 1
                var products = controller.GetProducts();
                var product = products.Single(p => p.Name == "Latte");
                var order = controller.OrderCoffee(product);
                bool isFinished = controller.InsertCoin(order, 100);
                Assert.AreEqual(true, isFinished, "100 Cent genügen");
                Assert.AreEqual(100, order.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(100 - product.PriceInCents, order.ReturnCents);
                Assert.AreEqual(0, order.DonationCents);
                Assert.AreEqual("50", order.ReturnCoinValues);

                //Latte2
                products = controller.GetProducts();
                product = products.Single(p => p.Name == "Latte");
                order = controller.OrderCoffee(product);
                isFinished = controller.InsertCoin(order, 100);
                Assert.AreEqual(true, isFinished, "100 Cent genügen");
                Assert.AreEqual(100, order.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(100 - product.PriceInCents, order.ReturnCents);
                Assert.AreEqual(0, order.DonationCents);
                Assert.AreEqual("50", order.ReturnCoinValues);

                //Latte3
                products = controller.GetProducts();
                product = products.Single(p => p.Name == "Latte");
                order = controller.OrderCoffee(product);
                isFinished = controller.InsertCoin(order, 100);
                Assert.AreEqual(true, isFinished, "100 Cent genügen");
                Assert.AreEqual(100, order.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(100 - product.PriceInCents, order.ReturnCents);
                Assert.AreEqual(0, order.DonationCents);
                Assert.AreEqual("50", order.ReturnCoinValues);

                //Latte4
                products = controller.GetProducts();
                product = products.Single(p => p.Name == "Latte");
                order = controller.OrderCoffee(product);
                isFinished = controller.InsertCoin(order, 100);
                Assert.AreEqual(true, isFinished, "100 Cent genügen");
                Assert.AreEqual(100, order.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(100 - product.PriceInCents, order.ReturnCents);
                Assert.AreEqual(0, order.DonationCents);
                Assert.AreEqual("20;20;10", order.ReturnCoinValues);

                //Latte5
                products = controller.GetProducts();
                product = products.Single(p => p.Name == "Latte");
                order = controller.OrderCoffee(product);
                isFinished = controller.InsertCoin(order, 100);
                Assert.AreEqual(true, isFinished, "100 Cent genügen");
                Assert.AreEqual(100, order.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(100 - product.PriceInCents, order.ReturnCents);
                Assert.AreEqual(0, order.DonationCents);
                Assert.AreEqual("20;10;10;5;5", order.ReturnCoinValues);

                //Latte6
                products = controller.GetProducts();
                product = products.Single(p => p.Name == "Latte");
                order = controller.OrderCoffee(product);
                isFinished = controller.InsertCoin(order, 100);
                Assert.AreEqual(true, isFinished, "100 Cent genügen");
                Assert.AreEqual(100, order.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(100 - product.PriceInCents - order.DonationCents, order.ReturnCents);
                Assert.AreEqual(45, order.DonationCents);
                Assert.AreEqual("5", order.ReturnCoinValues);

                // Depot überprüfen
                var coins = controller.GetCoinDepot().ToArray();
                int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
                Assert.AreEqual(1500, sumOfCents, "Beim Start sind 1155 Cents + 300 Cents für Latte + 45 Cents Donation");
                Assert.AreEqual("3*200 + 9*100 + 0*50 + 0*20 + 0*10 + 0*5", controller.GetCoinDepotString());

                var orders = controller.GetAllOrdersWithProduct().ToArray();

                Assert.AreEqual(6, orders.Length, "Es sind genau sechs Bestellungen");
                Assert.AreEqual(0, orders[0].DonationCents, "Keine Spende");
                Assert.AreEqual(0, orders[1].DonationCents, "Keine Spende");
                Assert.AreEqual(0, orders[2].DonationCents, "Keine Spende");
                Assert.AreEqual(0, orders[3].DonationCents, "Keine Spende");
                Assert.AreEqual(0, orders[4].DonationCents, "Keine Spende");
                Assert.AreEqual(45, orders[5].DonationCents, "45 Cents Spende");

                Assert.AreEqual(100, orders[0].ThrownInCents, "100 Cents wurden eingeworfen");
                Assert.AreEqual(100, orders[1].ThrownInCents, "100 Cents wurden eingeworfen");
                Assert.AreEqual(100, orders[2].ThrownInCents, "100 Cents wurden eingeworfen");
                Assert.AreEqual(100, orders[3].ThrownInCents, "100 Cents wurden eingeworfen");
                Assert.AreEqual(100, orders[4].ThrownInCents, "100 Cents wurden eingeworfen");
                Assert.AreEqual(100, orders[5].ThrownInCents, "100 Cents wurden eingeworfen");

                Assert.AreEqual("Latte", orders[0].Product.Name, "Produktname Latte");
                Assert.AreEqual("Latte", orders[1].Product.Name, "Produktname Latte");
                Assert.AreEqual("Latte", orders[2].Product.Name, "Produktname Latte");
                Assert.AreEqual("Latte", orders[3].Product.Name, "Produktname Latte");
                Assert.AreEqual("Latte", orders[4].Product.Name, "Produktname Latte");
                Assert.AreEqual("Latte", orders[5].Product.Name, "Produktname Latte");
            }
        }

    }
}
