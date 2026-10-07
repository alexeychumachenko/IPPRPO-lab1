using System;
using System.Collections.Generic;
using System.Windows.Forms;
  
public class Log
{
	private CurrencyConverter _converter;

	public Log()
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
}