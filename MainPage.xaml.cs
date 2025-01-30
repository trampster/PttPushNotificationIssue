
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PttPushNotificationIssue;

public class PttPushViewModel : INotifyPropertyChanged
{
	readonly IPTChannelService _ptChannelService;

	public PttPushViewModel(IPTChannelService ptChannelService)
	{
		_ptChannelService = ptChannelService;
		_ptChannelService.ApnsTokenChanged += OnApnsTokenChanged;
		_ptChannelService.ReceivedPush += OnReceivedPush;
	}

    void OnReceivedPush(object? sender, EventArgs e)
    {
        Log += $"{DateTime.Now} Push Received {Environment.NewLine}";
    }

	string _log = "";

	public string Log
	{
		get => _log;
		set => SetProperty(ref _log, value);
	}

    void SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
	{
		backingStore = value;
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	void OnApnsTokenChanged(object? sender, string token)
	{
		ApnsToken = token;
	}

	string _apnsToken = "asdf";

	public event PropertyChangedEventHandler? PropertyChanged;

	public string ApnsToken
	{
		get => _apnsToken;
		set => SetProperty(ref _apnsToken, value);
	}

	public Task Initialize()
	{
		return _ptChannelService.SetupChannel();
	}
}

public partial class MainPage : ContentPage
{
	readonly PttPushViewModel _viewModel;

	public MainPage(PttPushViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _viewModel.Initialize();
	}
}

