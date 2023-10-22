using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Reactive.Bindings.Disposables;
using Reactive.Bindings.Extensions;

namespace MouseKnightGD.Title;

/// <summary>
/// タイトル画面のセッション。
/// </summary>
public partial class TitleSession : Node
{
	[Export] private StartField _startField;
	[Export] private ExitField _exitField;
	[Export] private Sprite2D _titleSprite;
	
	public async Task<TitleSessionResult> Run(CancellationToken ct)
	{
		CompositeDisposable disposable = new();
		TaskCompletionSource<TitleSessionResult> tcs = new();
		_startField.Initialize();
		_startField.OnStart.FirstAsync().Subscribe(_ => OnStart(tcs)).AddTo(disposable);
		_exitField.Initialize();
		_exitField.OnExit.FirstAsync().Subscribe(_ => OnExit(tcs)).AddTo(disposable);
		
		var result = await tcs.Task;
		
		_startField.Close();
		_exitField.Close();
		disposable.Dispose();
		await Task.Delay(TimeSpan.FromSeconds(0.5f), ct);
		
		return result;
	}

	private void OnStart(TaskCompletionSource<TitleSessionResult> tcs)
	{
		_titleSprite.Modulate = new Color(1.0f, 1.0f, 1.0f, 1.0f);
		var sessionResult = new TitleSessionResult(TitleSessionResult.ResultType.BeginGame);
		tcs.TrySetResult(sessionResult);
	}
	
	private void OnExit(TaskCompletionSource<TitleSessionResult> tcs)
	{
		var sessionResult = new TitleSessionResult(TitleSessionResult.ResultType.Exit);
		tcs.TrySetResult(sessionResult);
	}
}