using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ThreadedProcess
{
    public static bool EnableThreading = false;

    private bool m_IsDone = false;

    private object m_Handle = new object();

    private System.Threading.Thread m_Thread = null;

    public bool IsDone
    {
        get
        {
            bool tmp;
            lock (m_Handle)
            {
                tmp = m_IsDone;
            }
            return tmp;
        }
        set
        {
            lock (m_Handle)
            {
                m_IsDone = value;
            }
        }
    }

    public virtual void Start()
    {
        if (EnableThreading)
        {
            m_Thread = new System.Threading.Thread(Run);
            m_Thread.Start();
        }
        else
        {
            Run();
        }
    }

    public virtual void Abort()
    {
        if(m_Thread != null)
        {
            m_Thread.Abort();
        }
    }

    public virtual bool Update()
    {
        if (IsDone)
        {
            OnFinished();
            return true;
        }
        return false;
    }

    public IEnumerator WaitForComplete()
    {
        while (!Update())
        {
            yield return null;
        }
    }

    protected abstract void ThreadFunction();

    protected abstract void OnFinished();

    private void Run()
    {
        ThreadFunction();
        IsDone = true;
    }
}
