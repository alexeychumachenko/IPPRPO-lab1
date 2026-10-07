using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace lab1_9_
{
    public partial class Form1 : Form
    {
        private CurrencyConverter _converter;

        public Form1()
        {
            InitializeComponent();

            var cbr = new CbrProvider();
            var google = new GoogleFinanceProvider();
            var openExchange = new OpenExchangeProvider();

            var providers = new List<ICurrencyProvider> { cbr, google, openExchange };
            _converter = new CurrencyConverter(providers, Log);

            Log("Система инициализирована. Источники: ЦБ РФ, Google, OpenExchange.");

            cmbFrom.SelectedIndex = 0;
            cmbTo.SelectedIndex = 1;
        }

        private void Log(string message)
        {
            lstLog.Items.Add(message);
            lstLog.TopIndex = lstLog.Items.Count - 1;
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            lstLog.Items.Clear();
            try
            {

                if (!decimal.TryParse(txtAmount.Text, out decimal amount))
                {
                    MessageBox.Show("Введите корректную сумму.", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string from = cmbFrom.SelectedItem.ToString();
                string to = cmbTo.SelectedItem.ToString();

                var result = _converter.Convert(from, to, amount);
                textBox1.Text = $"{ result.Value:F2}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибочка!");
            }

        }

        private void btnAddManual_Click(object sender, EventArgs e)
        {
            try
            {
                var manual = new ManualProvider("USD", "EUR", decimal.Parse(ManualCourseTextBox.Text));
                _converter.AddProvider(manual);
                Log("Расширение выполнено");
                Log("Теперь при конвертации USD->EUR будет использован ManualProvider.");

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибочка!");
            }
        }
    }

    public interface ICurrencyProvider
    {
        string Name { get; }
        decimal? GetRate(string fromCurrency, string toCurrency);
    }

    public class CbrProvider : ICurrencyProvider
    {
        public string Name => "ЦБ РФ";

        public decimal? GetRate(string fromCurrency, string toCurrency)
        {
            if (fromCurrency != "RUB" && toCurrency != "RUB")
                return null;

            var ratesToRub = new Dictionary<string, decimal>
            {
                { "USD", 92.50m },
                { "EUR", 100.30m },
                { "GBP", 117.80m },
                { "JPY", 0.61m },
                { "RUB", 1m }
            };

            if (!ratesToRub.ContainsKey(fromCurrency) || !ratesToRub.ContainsKey(toCurrency))
                return null;

            return ratesToRub[fromCurrency] / ratesToRub[toCurrency];
        }
    }

    public class GoogleFinanceProvider : ICurrencyProvider
    {
        public string Name => "Google Finance";

        public decimal? GetRate(string fromCurrency, string toCurrency)
        {
            if (fromCurrency == "GBP" || toCurrency == "GBP")
                return null;

            var rates = new Dictionary<string, decimal>
            {
                { "USD", 1m },
                { "EUR", 0.92m },
                { "RUB", 92.80m },
                { "JPY", 151.20m }
            };

            if (!rates.ContainsKey(fromCurrency) || !rates.ContainsKey(toCurrency))
                return null;

            return rates[toCurrency] / rates[fromCurrency];
        }
    }

    public class OpenExchangeProvider : ICurrencyProvider
    {
        public string Name => "OpenExchangeRates";

        public decimal? GetRate(string fromCurrency, string toCurrency)
        {
            var rates = new Dictionary<string, decimal>
            {
                { "USD", 1m },
                { "EUR", 0.91m },
                { "RUB", 93.10m },
                { "GBP", 0.79m },
                { "JPY", 150.80m }
            };

            if (!rates.ContainsKey(fromCurrency) || !rates.ContainsKey(toCurrency))
                return null;

            return rates[toCurrency] / rates[fromCurrency];
        }
    }

    public class ManualProvider : ICurrencyProvider
    {
        private readonly decimal _fixedRate;
        private readonly string _from;
        private readonly string _to;

        public string Name => "Manual (ручной ввод)";

        public ManualProvider(string from, string to, decimal rate)
        {
            _from = from;
            _to = to;
            _fixedRate = rate;
        }

        public decimal? GetRate(string fromCurrency, string toCurrency)
        {
            if (fromCurrency == _from && toCurrency == _to)
                return _fixedRate;
            return null;
        }
    }

    public class CurrencyConverter
    {
        private readonly List<ICurrencyProvider> _providers;
        private readonly Action<string> _log;

        public CurrencyConverter(IEnumerable<ICurrencyProvider> providers, Action<string> log)
        {
            _providers = new List<ICurrencyProvider>(providers);
            _log = log;
        }

        public void AddProvider(ICurrencyProvider provider)
        {
            _providers.Insert(0, provider);
            _log($"[+] Добавлен новый источник: {provider.Name}");
        }

        public decimal? Convert(string from, string to, decimal amount)
        {
            _log($"--- Конвертация {amount} {from} -> {to} ---");

            foreach (var provider in _providers)
            {
                try
                {
                    var rate = provider.GetRate(from, to);
                    if (rate.HasValue)
                    {
                        _log($"  [OK] {provider.Name}: курс = {rate.Value:F4}");
                        return amount * rate.Value;
                    }
                    else
                    {
                        _log($"  [--] {provider.Name}: курс не найден");
                    }
                }
                catch (Exception ex)
                {
                    _log($"  [ERR] {provider.Name}: {ex.Message}");
                }
            }

            _log("  [!] Ни один источник не дал курс.");
            return null;
        }
    }
}