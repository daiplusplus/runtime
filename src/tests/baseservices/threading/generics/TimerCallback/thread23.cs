// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Threading;

interface IGen<T>
{
	void Target<U>(object p);
	T Dummy(T t);
}

struct GenInt : IGen<int>
{
	public int Dummy(int t) { return t; }

	public void Target<U>(object p)
	{			
		if (Test_thread23.Xcounter>=Test_thread23.nThreads)
		{
			ManualResetEvent evt = (ManualResetEvent) p;	
			evt.Set();
		}
		else
		{
			Interlocked.Increment(ref Test_thread23.Xcounter);	
		}
	}
	
	public static void ThreadPoolTest<U>()
	{
		ManualResetEvent evt = new ManualResetEvent(false);		
		
		IGen<int> obj = new GenInt();

		TimerCallback tcb = new TimerCallback(obj.Target<U>);
		Timer timer = new Timer(tcb,evt,Test_thread23.delay,Test_thread23.period);
	
		evt.WaitOne();
		timer.Dispose();
		Test_thread23.Eval(Test_thread23.Xcounter>=Test_thread23.nThreads);
		Test_thread23.Xcounter = 0;
	}
}

struct GenDouble : IGen<double>
{
	public double Dummy(double t) { return t; }

	public void Target<U>(object p)
	{			
		if (Test_thread23.Xcounter>=Test_thread23.nThreads)
		{
			ManualResetEvent evt = (ManualResetEvent) p;	
			evt.Set();
		}
		else
		{
			Interlocked.Increment(ref Test_thread23.Xcounter);	
		}
	}
	
	public static void ThreadPoolTest<U>()
	{
		ManualResetEvent evt = new ManualResetEvent(false);		
		
		IGen<double> obj = new GenDouble();

		TimerCallback tcb = new TimerCallback(obj.Target<U>);
		Timer timer = new Timer(tcb,evt,Test_thread23.delay,Test_thread23.period);
	
		evt.WaitOne();
		timer.Dispose();
		Test_thread23.Eval(Test_thread23.Xcounter>=Test_thread23.nThreads);
		Test_thread23.Xcounter = 0;
	}
}

struct GenString : IGen<string>
{
	public string Dummy(string t) { return t; }

	public void Target<U>(object p)
	{			
		if (Test_thread23.Xcounter>=Test_thread23.nThreads)
		{
			ManualResetEvent evt = (ManualResetEvent) p;	
			evt.Set();
		}
		else
		{
			Interlocked.Increment(ref Test_thread23.Xcounter);	
		}
	}
	
	public static void ThreadPoolTest<U>()
	{
		ManualResetEvent evt = new ManualResetEvent(false);		
		
		IGen<string> obj = new GenString();

		TimerCallback tcb = new TimerCallback(obj.Target<U>);
		Timer timer = new Timer(tcb,evt,Test_thread23.delay,Test_thread23.period);
	
		evt.WaitOne();
		timer.Dispose();
		Test_thread23.Eval(Test_thread23.Xcounter>=Test_thread23.nThreads);
		Test_thread23.Xcounter = 0;
	}
}

struct GenObject : IGen<object>
{
	public object Dummy(object t) { return t; }

	public void Target<U>(object p)
	{			
		if (Test_thread23.Xcounter>=Test_thread23.nThreads)
		{
			ManualResetEvent evt = (ManualResetEvent) p;	
			evt.Set();
		}
		else
		{
			Interlocked.Increment(ref Test_thread23.Xcounter);	
		}
	}
	
	public static void ThreadPoolTest<U>()
	{
		ManualResetEvent evt = new ManualResetEvent(false);		
		
		IGen<object> obj = new GenObject();

		TimerCallback tcb = new TimerCallback(obj.Target<U>);
		Timer timer = new Timer(tcb,evt,Test_thread23.delay,Test_thread23.period);
	
		evt.WaitOne();
		timer.Dispose();
		Test_thread23.Eval(Test_thread23.Xcounter>=Test_thread23.nThreads);
		Test_thread23.Xcounter = 0;
	}
}

struct GenGuid : IGen<Guid>
{
	public Guid Dummy(Guid t) { return t; }

	public void Target<U>(object p)
	{			
		if (Test_thread23.Xcounter>=Test_thread23.nThreads)
		{
			ManualResetEvent evt = (ManualResetEvent) p;	
			evt.Set();
		}
		else
		{
			Interlocked.Increment(ref Test_thread23.Xcounter);	
		}
	}
	
	public static void ThreadPoolTest<U>()
	{
		ManualResetEvent evt = new ManualResetEvent(false);		
		
		IGen<Guid> obj = new GenGuid();

		TimerCallback tcb = new TimerCallback(obj.Target<U>);
		Timer timer = new Timer(tcb,evt,Test_thread23.delay,Test_thread23.period);
	
		evt.WaitOne();
		timer.Dispose();
		Test_thread23.Eval(Test_thread23.Xcounter>=Test_thread23.nThreads);
		Test_thread23.Xcounter = 0;
	}
}

public class Test_thread23
{
	public static int delay = 0;
	public static int period = 2;
	public static int nThreads = 5;
	public static int counter = 0;
	public static int Xcounter = 0;
	public static bool result = true;
	public static void Eval(bool exp)
	{
		counter++;
		if (!exp)
		{
			result = exp;
			Console.WriteLine("Test Failed at location: " + counter);
		}
	
	}
	
	public static int Main()
	{
	
		GenInt.ThreadPoolTest<int>();
		GenDouble.ThreadPoolTest<int>();
		GenString.ThreadPoolTest<int>();
		GenObject.ThreadPoolTest<int>(); 
		GenGuid.ThreadPoolTest<int>(); 

		GenInt.ThreadPoolTest<double>();
		GenDouble.ThreadPoolTest<double>();
		GenString.ThreadPoolTest<double>();
		GenObject.ThreadPoolTest<double>(); 
		GenGuid.ThreadPoolTest<double>(); 

		GenInt.ThreadPoolTest<string>();
		GenDouble.ThreadPoolTest<string>();
		GenString.ThreadPoolTest<string>();
		GenObject.ThreadPoolTest<string>(); 
		GenGuid.ThreadPoolTest<string>(); 

		GenInt.ThreadPoolTest<object>();
		GenDouble.ThreadPoolTest<object>();
		GenString.ThreadPoolTest<object>();
		GenObject.ThreadPoolTest<object>(); 
		GenGuid.ThreadPoolTest<object>(); 

		GenInt.ThreadPoolTest<Guid>();
		GenDouble.ThreadPoolTest<Guid>();
		GenString.ThreadPoolTest<Guid>();
		GenObject.ThreadPoolTest<Guid>(); 
		GenGuid.ThreadPoolTest<Guid>(); 
		
		if (result)
		{
			Console.WriteLine("Test Passed");
			return 100;
		}
		else
		{
			Console.WriteLine("Test Failed");
			return 1;
		}
	}
}		


