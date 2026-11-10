using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Serilog;
using Newtonsoft.Json;
using System.Linq;
using FluentValidation;

using System.IO;



public static class AppLog
{
    public static readonly ILogger Logger;

    static AppLog()
    {
        Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/app.log")
            .CreateLogger();
    }

    public static void Close()
    {
        Log.CloseAndFlush();
    }
}



namespace lab1_9_
{
    public partial class Form1 : Form
    {
        private CurrencyConverter _converter;

        public Form1()
        {
			try
			{
				InitializeComponent();
				AppLog.Logger.Information("Application started");

				var cbr = new CbrProvider();
				var google = new GoogleFinanceProvider();
				var openExchange = new OpenExchangeProvider();

				var providers = new List<ICurrencyProvider> { cbr, google, openExchange };
				
				
				var repository = new JsonProvidersRepository(
					Path.Combine(AppContext.BaseDirectory, "providers.json"));

				_converter = new CurrencyConverter(providers, Log2, repository);

				Log2("Система инициализирована. Источники: ЦБ РФ, Google, OpenExchange.");
				AppLog.Logger.Information("Система инициализирована. Источники: ЦБ РФ, Google, OpenExchange.");

				cmbFrom.SelectedIndex = 0;
				cmbTo.SelectedIndex = 1;
				
			}
			catch (Exception ex)
			{
				AppLog.Logger.Fatal(ex, "Application crashed");
			}
			finally
			{
				//AppLog.Close();
			}
        }

        private void Log2(string message)
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

				var request = new ConversionRequest
				{
					From = cmbFrom.SelectedItem.ToString(),
					To = cmbTo.SelectedItem.ToString(),
					Amount = amount
				};

				var validator = new ConversionRequestValidator();
				var result = validator.Validate(request);

				if (!result.IsValid)
				{
					var errors = string.Join("\n", result.Errors.Select(e => e.ErrorMessage));
					MessageBox.Show(errors, "Ошибка валидации");
					return;
				}

				var converted = _converter.Convert(request.From, request.To, request.Amount);
				textBox1.Text = $"{converted.Value:F2}";
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
                Log2("Расширение выполнено");
				AppLog.Logger.Information("Расширение выполнено");
                Log2("Теперь при конвертации USD->EUR будет использован ManualProvider.");
				AppLog.Logger.Information("Теперь при конвертации USD->EUR будет использован ManualProvider.");

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
		
		private readonly IProvidersRepository _repository;

        public CurrencyConverter(IEnumerable<ICurrencyProvider> providers, Action<string> log, IProvidersRepository repository)
        {
            _providers = new List<ICurrencyProvider>(providers);
            _log = log;
			_repository = repository;
        }

        public void AddProvider(ICurrencyProvider provider)
        {
            _providers.Insert(0, provider);
            _log($"[+] Добавлен новый источник: {provider.Name}");
			AppLog.Logger.Information("[+] Добавлен новый источник: {providerName}", provider.Name);
        }

        public decimal? Convert(string from, string to, decimal amount)
        {
			_repository.Save(_providers);
			
            _log($"--- Конвертация {amount} {from} -> {to} ---");
			AppLog.Logger.Information("--- Конвертация {amount} {from} -> {to} ---", amount, from, to);

            foreach (var provider in _providers)
            {
                try
                {
                    var rate = provider.GetRate(from, to);
                    if (rate.HasValue)
                    {
                        _log($"  [OK] {provider.Name}: курс = {rate.Value:F4}");
						AppLog.Logger.Information("  [OK] {providerName}: курс = {rateValue:F4}", provider.Name, rate.Value);
                        return amount * rate.Value;
                    }
                    else
                    {
                        _log($"  [--] {provider.Name}: курс не найден");
						AppLog.Logger.Information("  [--] {providerName}: курс не найден", provider.Name);
                    }
                }
                catch (Exception ex)
                {
                    _log($"  [ERR] {provider.Name}: {ex.Message}");
					AppLog.Logger.Information("  [ERR] {providerName}: {exMessage}", provider.Name, ex.Message);
                }
            }

            _log("  [!] Ни один источник не дал курс.");
			AppLog.Logger.Information("  [!] Ни один источник не дал курс.");
			
			
			
            return null;
        }
    }
	
	public interface IProvidersRepository
    {
        void Save(IEnumerable<ICurrencyProvider> providers);
    }
	
	public class JsonProvidersRepository : IProvidersRepository
    {
        private readonly string _path;
        public JsonProvidersRepository(string path) => _path = path;

        public void Save(IEnumerable<ICurrencyProvider> providers)
        {
            var options = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            };
            string json = JsonConvert.SerializeObject(providers, options);

            var dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(_path, json);
        }
    }
	
	public class ConversionRequest
    {
        public string From { get; set; }
        public string To { get; set; }
        public decimal Amount { get; set; }
    }
	
	public class ConversionRequestValidator : AbstractValidator<ConversionRequest>
    {
        private static readonly string[] SupportedCurrencies =
            { "USD", "EUR", "RUB", "GBP", "JPY" };

        public ConversionRequestValidator()
        {
            RuleFor(x => x.From)
                .NotEmpty().WithMessage("Валюта-источник обязательна")
                .Must(c => SupportedCurrencies.Contains(c))
                .WithMessage("Неподдерживаемая валюта-источник");

            RuleFor(x => x.To)
                .NotEmpty().WithMessage("Валюта-назначение обязательна")
                .Must(c => SupportedCurrencies.Contains(c))
                .WithMessage("Неподдерживаемая валюта-назначение");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Сумма должна быть больше нуля")
                .LessThanOrEqualTo(1_000_000_000m)
                .WithMessage("Сумма слишком велика");

            RuleFor(x => x)
                .Must(x => x.From != x.To)
                .WithMessage("Валюты не должны совпадать")
                .OverridePropertyName("FromTo");
        }
    }
}