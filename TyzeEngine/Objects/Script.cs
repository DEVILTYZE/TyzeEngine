using System;
using System.Threading.Tasks;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public abstract class Script : IScript
{
    private readonly Task _task;
    private bool _isWorking;
    private bool _isPaused;

    public bool ExceptionWhileExecuting { get; private set; }
    public IGameObject Object { get; }

    protected Script(IGameObject obj)
    {
        ExceptionWhileExecuting = false;
        Object = obj;
        _isWorking = false;
        _isPaused = false;
        _task = new Task(() => {
            while (_isWorking)
            {
                try
                {
                    Execute();
                }
                catch (Exception)
                {
                    ExceptionWhileExecuting = true;
                }
                
                while (_isPaused)
                    Task.Delay(ConstHelper.DefaultDelay);
            }
        });
    }

    public void Start()
    {
        _isWorking = true;
        
        if (!_isPaused && _task.Status != TaskStatus.Running)
            _task.Start();
        else
            _isPaused = false;
    }
    
    protected abstract void Execute();

    public void Pause() => _isPaused = true;

    public void Stop()
    {
        _isWorking = false;
        _isPaused = false;
    }
}