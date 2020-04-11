﻿using System;
using System.Reflection;
using Mono.Linker.Tests.Cases.Expectations.Assertions;
using Mono.Linker.Tests.Cases.Expectations.Metadata;

namespace Mono.Linker.Tests.Cases.Reflection {

	[SetupCSharpCompilerToUse ("csc")]
	public class MethodUsedViaReflection {
		public static void Main ()
		{
			TestName ();
			TestNameAndExplicitBindingFlags ();
			TestNameAndType ();
			TestNameBindingFlagsAndParameterModifier ();
			TestNameBindingFlagsCallingConventionParameterModifier ();
#if NETCOREAPP
			TestNameWithIntAndType ();
			TestNameWithIntAndBindingFlags ();
			TestNameWithIntBindingFlagsCallingConventionParameter ();
#endif
			TestNullName ();
			TestEmptyName ();
			TestNonExistingName ();
			TestNullType ();
			TestDataFlowType ();
			TestIfElse (1);
		}

		[Kept]
		[RecognizedReflectionAccessPattern (
			typeof (Type), nameof (Type.GetMethod), new Type [] { typeof (string) },
			typeof (MethodUsedViaReflection), nameof (MethodUsedViaReflection.OnlyCalledViaReflection), new Type [0])]
		static void TestName ()
		{
			var method = typeof (MethodUsedViaReflection).GetMethod ("OnlyCalledViaReflection");
			method.Invoke (null, new object [] { });
		}
		[Kept]
		[RecognizedReflectionAccessPattern (
			typeof (Type), nameof (Type.GetMethod), new Type [] { typeof (string), typeof (BindingFlags) },
			typeof (TestNameAndExplicitBindingClass), nameof (TestNameAndExplicitBindingClass.OnlyCalledViaReflection), new Type [0])]
		static void TestNameAndExplicitBindingFlags ()
		{
			var method = typeof (TestNameAndExplicitBindingClass).GetMethod ("OnlyCalledViaReflection", BindingFlags.Static | BindingFlags.NonPublic);
			method.Invoke (null, new object [] { });
		}

		[Kept]
		static void TestNameAndType ()
		{
			var method = typeof (TestNameAndTypeClass).GetMethod ("OnlyCalledViaReflection", new Type [] { typeof (int) });
			method.Invoke (null, new object [] { });
		}

		[Kept]
		static void TestNameBindingFlagsAndParameterModifier()
		{
			var method = typeof (TestNameBindingFlagsAndParameterClass).GetMethod ("OnlyCalledViaReflection", BindingFlags.Public, null, new Type [] { }, null);
			method.Invoke (null, new object [] { });
		}

		[Kept]
		static void TestNameBindingFlagsCallingConventionParameterModifier()
		{
			var method = typeof (TestNameBindingFlagsCallingConventionParameterClass).GetMethod ("OnlyCalledViaReflection", BindingFlags.NonPublic, null, CallingConventions.Standard, new Type [] { }, null);
		}
#if NETCOREAPP
		[Kept]
		static void TestNameWithIntAndType ()
		{
			var method = typeof (TestNameWithIntAndTypeClass).GetMethod ("OnlyCalledViaReflection", 1, new Type [] { typeof (int) });
			method.Invoke (null, new object [] { });
		}

		[Kept]
		static void TestNameWithIntAndBindingFlags ()
		{
			var method = typeof (TestNameWithIntAndBindingClass).GetMethod ("OnlyCalledViaReflection", 1, BindingFlags.Public, null, new Type [] { }, null);
			method.Invoke (null, new object [] { });
		}

		[Kept]
		static void TestNameWithIntBindingFlagsCallingConventionParameter()
		{
			var method = typeof (TestNameWithIntBindingFlagsCallingConventionParameterClass).GetMethod ("OnlyCalledViaReflection", 1, BindingFlags.Static | BindingFlags.NonPublic, null, CallingConventions.Any, new Type [] { }, null);
			method.Invoke (null, new object [] { });
		}
#endif

		[Kept]
		static void TestNullName ()
		{
			var method = typeof (MethodUsedViaReflection).GetMethod (null);
		}

		[Kept]
		static void TestEmptyName ()
		{
			var method = typeof (MethodUsedViaReflection).GetMethod (string.Empty);
		}

		[Kept]
		static void TestNonExistingName ()
		{
			var method = typeof (MethodUsedViaReflection).GetMethod ("NonExisting");
		}

		[Kept]
		static void TestNullType ()
		{
			Type type = null;
			var method = type.GetMethod ("OnlyCalledViaReflection", BindingFlags.Static | BindingFlags.NonPublic);
		}

		[Kept]
		static Type FindType ()
		{
			return typeof (MethodUsedViaReflection);
		}

		[Kept]
		[UnrecognizedReflectionAccessPattern (
			typeof (Type), nameof (Type.GetMethod), new Type [] { typeof (string), typeof (BindingFlags) })]
		static void TestDataFlowType ()
		{
			Type type = FindType ();
			var method = type.GetMethod ("OnlyCalledViaReflection", BindingFlags.Static | BindingFlags.NonPublic);
		}

		[Kept]
		static void TestIfElse (int i)
		{
			Type myType;
			if (i == 1) {
				myType = typeof (IfClass);
			} else if (i == 2) {
				myType = typeof (ElseIfClass);
			} else {
				myType = typeof (ElseClass);
			}
			string mystring;
			if (i == 1) {
				mystring = "OnlyCalledViaReflection";
			} else if (i == 2) {
				mystring = "ElseIfCall";
			} else {
				mystring = null;
			}
			var method = myType.GetMethod (mystring, BindingFlags.Static, null, new Type [] { typeof (int) }, null);
			method.Invoke (null, new object [] { });
		}

		[Kept]
		private static int OnlyCalledViaReflection ()
		{
			return 42;
		}
		[Kept]
		private int OnlyCalledViaReflection (int foo)
		{
			return 43;
		}
		[Kept]
		public int OnlyCalledViaReflection (int foo, int bar)
		{
			return 44;
		}
		[Kept]
		public static int OnlyCalledViaReflection (int foo, int bar, int baz)
		{
			return 45;
		}
		[Kept]
		private class TestNameAndExplicitBindingClass
		{
			[Kept]
			private static int OnlyCalledViaReflection ()
			{
				return 42;
			}

			private int OnlyCalledViaReflection (int foo)
			{
				return 43;
			}

			public int OnlyCalledViaReflection (int foo, int bar)
			{
				return 44;
			}

			public static int OnlyCalledViaReflection (int foo, int bar, int baz)
			{
				return 45;
			}
		}

		[Kept]
		private class TestNameAndTypeClass
		{
			[Kept]
			private static int OnlyCalledViaReflection ()
			{
				return 42;
			}
			[Kept]
			private int OnlyCalledViaReflection (int foo)
			{
				return 43;
			}
			[Kept]
			public int OnlyCalledViaReflection (int foo, int bar)
			{
				return 44;
			}
			[Kept]
			public static int OnlyCalledViaReflection (int foo, int bar, int baz)
			{
				return 45;
			}
		}

		[Kept]
		private class TestNameBindingFlagsAndParameterClass
		{
			private static int OnlyCalledViaReflection ()
			{
				return 42;
			}
			
			private int OnlyCalledViaReflection (int foo)
			{
				return 43;
			}
			[Kept]
			public int OnlyCalledViaReflection (int foo, int bar)
			{
				return 44;
			}
			[Kept]
			public static int OnlyCalledViaReflection (int foo, int bar, int baz)
			{
				return 45;
			}
		}
		[Kept]
		private class TestNameBindingFlagsCallingConventionParameterClass
		{
			[Kept]
			private static int OnlyCalledViaReflection ()
			{
				return 42;
			}
			[Kept]
			private int OnlyCalledViaReflection (int foo)
			{
				return 43;
			}
			public int OnlyCalledViaReflection (int foo, int bar)
			{
				return 44;
			}
			public static int OnlyCalledViaReflection (int foo, int bar, int baz)
			{
				return 45;
			}
		}

#if NETCOREAPP
		[Kept]
		private class TestNameWithIntAndTypeClass
		{
			[Kept]
			private static int OnlyCalledViaReflection ()
			{
				return 42;
			}
			[Kept]
			private int OnlyCalledViaReflection (int foo)
			{
				return 43;
			}
			[Kept]
			public int OnlyCalledViaReflection (int foo, int bar)
			{
				return 44;
			}
			[Kept]
			public static int OnlyCalledViaReflection (int foo, int bar, int baz)
			{
				return 45;
			}
		}

		[Kept]
		private class TestNameWithIntAndBindingClass
		{
			private static int OnlyCalledViaReflection ()
			{
				return 42;
			}
			private int OnlyCalledViaReflection (int foo)
			{
				return 43;
			}
			[Kept]
			public int OnlyCalledViaReflection (int foo, int bar)
			{
				return 44;
			}
			[Kept]
			public static int OnlyCalledViaReflection (int foo, int bar, int baz)
			{
				return 45;
			}
		}

		[Kept]
		private class TestNameWithIntBindingFlagsCallingConventionParameterClass
		{
			[Kept]
			private static int OnlyCalledViaReflection ()
			{
				return 42;
			}
			private int OnlyCalledViaReflection (int foo)
			{
				return 43;
			}
			public int OnlyCalledViaReflection (int foo, int bar)
			{
				return 44;
			}
			public static int OnlyCalledViaReflection (int foo, int bar, int baz)
			{
				return 45;
			}
		}
#endif

		[Kept]
		private class IfClass
		{
			[Kept]
			public static int OnlyCalledViaReflection()
			{
				return 42;
			}
			
			private int OnlyCalledViaReflection(int foo)
			{
				return 43;
			}
			[Kept]
			public static int ElseIfCall()
			{
				return 44;
			}
		}

		[Kept]
		private class ElseIfClass
		{
			[Kept]
			public static int OnlyCalledViaReflection ()
			{
				return 45;
			}
			[Kept]
			private static int OnlyCalledViaReflection (int foo)
			{
				return 46;
			}
			[Kept]
			public static int ElseIfCall ()
			{
				return 47;
			}
		}

		[Kept]
		private class ElseClass
		{
			[Kept]
			public static int OnlyCalledViaReflection ()
			{
				return 48;
			}
			[Kept]
			private static int OnlyCalledViaReflection (int foo)
			{
				return 49;
			}
			private int ElseIfCall ()
			{
				return 50;
			}
		}
	}
}
