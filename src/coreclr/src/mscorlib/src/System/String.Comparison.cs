// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace System
{
    public partial class String
    {
        //
        //Native Static Methods
        //

        private unsafe static int CompareOrdinalIgnoreCaseHelper(String strA, String strB)
        {
            Contract.Requires(strA != null);
            Contract.Requires(strB != null);
            Contract.EndContractBlock();
            int length = Math.Min(strA.Length, strB.Length);

            fixed (char* ap = &strA._firstChar) fixed (char* bp = &strB._firstChar)
            {
                char* a = ap;
                char* b = bp;
                int charA = 0, charB = 0;

                while (length != 0)
                {
                    charA = *a;
                    charB = *b;

                    Debug.Assert((charA | charB) <= 0x7F, "strings have to be ASCII");

                    // uppercase both chars - notice that we need just one compare per char
                    if ((uint)(charA - 'a') <= (uint)('z' - 'a')) charA -= 0x20;
                    if ((uint)(charB - 'a') <= (uint)('z' - 'a')) charB -= 0x20;

                    //Return the (case-insensitive) difference between them.
                    if (charA != charB)
                        return charA - charB;

                    // Next char
                    a++; b++;
                    length--;
                }

                return strA.Length - strB.Length;
            }
        }

        // native call to COMString::CompareOrdinalEx
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int CompareOrdinalHelper(String strA, int indexA, int countA, String strB, int indexB, int countB);

        //This will not work in case-insensitive mode for any character greater than 0x80.  
        //We'll throw an ArgumentException.
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        unsafe internal static extern int nativeCompareOrdinalIgnoreCaseWC(String strA, sbyte* strBBytes);

        //
        //
        // NATIVE INSTANCE METHODS
        //
        //

        //
        // Search/Query methods
        //

        private unsafe static bool EqualsHelper(String strA, String strB)
        {
            Contract.Requires(strA != null);
            Contract.Requires(strB != null);
            Contract.Requires(strA.Length == strB.Length);

            int length = strA.Length;

            fixed (char* ap = &strA._firstChar) fixed (char* bp = &strB._firstChar)
            {
                char* a = ap;
                char* b = bp;

#if BIT64
                // Single int read aligns pointers for the following long reads
                // PERF: No length check needed as there is always an int32 worth of string allocated
                //       This read can also include the null terminator which both strings will have
                if (*(int*)a != *(int*)b) return false;
                length -= 2; a += 2; b += 2;

                // for AMD64 bit platform we unroll by 12 and
                // check 3 qword at a time. This is less code
                // than the 32 bit case and is a shorter path length.

                while (length >= 12)
                {
                    if (*(long*)a != *(long*)b) goto ReturnFalse;
                    if (*(long*)(a + 4) != *(long*)(b + 4)) goto ReturnFalse;
                    if (*(long*)(a + 8) != *(long*)(b + 8)) goto ReturnFalse;
                    length -= 12; a += 12; b += 12;
                }
#else
                while (length >= 10)
                {
                    if (*(int*)a != *(int*)b) goto ReturnFalse;
                    if (*(int*)(a + 2) != *(int*)(b + 2)) goto ReturnFalse;
                    if (*(int*)(a + 4) != *(int*)(b + 4)) goto ReturnFalse;
                    if (*(int*)(a + 6) != *(int*)(b + 6)) goto ReturnFalse;
                    if (*(int*)(a + 8) != *(int*)(b + 8)) goto ReturnFalse;
                    length -= 10; a += 10; b += 10;
                }
#endif

                // This depends on the fact that the String objects are
                // always zero terminated and that the terminating zero is not included
                // in the length. For odd string sizes, the last compare will include
                // the zero terminator.
                while (length > 0)
                {
                    if (*(int*)a != *(int*)b) goto ReturnFalse;
                    length -= 2; a += 2; b += 2;
                }

                return true;

            ReturnFalse:
                return false;
            }
        }

        private unsafe static bool EqualsIgnoreCaseAsciiHelper(String strA, String strB)
        {
            Contract.Requires(strA != null);
            Contract.Requires(strB != null);
            Contract.Requires(strA.Length == strB.Length);
            Contract.EndContractBlock();
            int length = strA.Length;

            fixed (char* ap = &strA._firstChar) fixed (char* bp = &strB._firstChar)
            {
                char* a = ap;
                char* b = bp;

                while (length != 0)
                {
                    int charA = *a;
                    int charB = *b;

                    Debug.Assert((charA | charB) <= 0x7F, "strings have to be ASCII");

                    // Ordinal equals or lowercase equals if the result ends up in the a-z range 
                    if (charA == charB ||
                       ((charA | 0x20) == (charB | 0x20) &&
                          (uint)((charA | 0x20) - 'a') <= (uint)('z' - 'a')))
                    {
                        a++;
                        b++;
                        length--;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private unsafe static bool StartsWithOrdinalHelper(String str, String startsWith)
        {
            Contract.Requires(str != null);
            Contract.Requires(startsWith != null);
            Contract.Requires(str.Length >= startsWith.Length);

            int length = startsWith.Length;

            fixed (char* ap = &str._firstChar) fixed (char* bp = &startsWith._firstChar)
            {
                char* a = ap;
                char* b = bp;

#if BIT64
                // Single int read aligns pointers for the following long reads
                // No length check needed as this method is called when length >= 2
                Debug.Assert(length >= 2);
                if (*(int*)a != *(int*)b) goto ReturnFalse;
                length -= 2; a += 2; b += 2;

                while (length >= 12)
                {
                    if (*(long*)a != *(long*)b) goto ReturnFalse;
                    if (*(long*)(a + 4) != *(long*)(b + 4)) goto ReturnFalse;
                    if (*(long*)(a + 8) != *(long*)(b + 8)) goto ReturnFalse;
                    length -= 12; a += 12; b += 12;
                }
#else
                while (length >= 10)
                {
                    if (*(int*)a != *(int*)b) goto ReturnFalse;
                    if (*(int*)(a+2) != *(int*)(b+2)) goto ReturnFalse;
                    if (*(int*)(a+4) != *(int*)(b+4)) goto ReturnFalse;
                    if (*(int*)(a+6) != *(int*)(b+6)) goto ReturnFalse;
                    if (*(int*)(a+8) != *(int*)(b+8)) goto ReturnFalse;
                    length -= 10; a += 10; b += 10;
                }
#endif

                while (length >= 2)
                {
                    if (*(int*)a != *(int*)b) goto ReturnFalse;
                    length -= 2; a += 2; b += 2;
                }

                // PERF: This depends on the fact that the String objects are always zero terminated 
                // and that the terminating zero is not included in the length. For even string sizes
                // this compare can include the zero terminator. Bitwise OR avoids a branch.
                return length == 0 | *a == *b;

            ReturnFalse:
                return false;
            }
        }

        private unsafe static int CompareOrdinalHelper(String strA, String strB)
        {
            Contract.Requires(strA != null);
            Contract.Requires(strB != null);

            // NOTE: This may be subject to change if eliminating the check
            // in the callers makes them small enough to be inlined by the JIT
            Debug.Assert(strA._firstChar == strB._firstChar,
                "For performance reasons, callers of this method should " +
                "check/short-circuit beforehand if the first char is the same.");

            int length = Math.Min(strA.Length, strB.Length);

            fixed (char* ap = &strA._firstChar) fixed (char* bp = &strB._firstChar)
            {
                char* a = ap;
                char* b = bp;

                // Check if the second chars are different here
                // The reason we check if _firstChar is different is because
                // it's the most common case and allows us to avoid a method call
                // to here.
                // The reason we check if the second char is different is because
                // if the first two chars the same we can increment by 4 bytes,
                // leaving us word-aligned on both 32-bit (12 bytes into the string)
                // and 64-bit (16 bytes) platforms.

                // For empty strings, the second char will be null due to padding.
                // The start of the string (not including sync block pointer)
                // is the method table pointer + string length, which takes up
                // 8 bytes on 32-bit, 12 on x64. For empty strings the null
                // terminator immediately follows, leaving us with an object
                // 10/14 bytes in size. Since everything needs to be a multiple
                // of 4/8, this will get padded and zeroed out.

                // For one-char strings the second char will be the null terminator.

                // NOTE: If in the future there is a way to read the second char
                // without pinning the string (e.g. System.Runtime.CompilerServices.Unsafe
                // is exposed to mscorlib, or a future version of C# allows inline IL),
                // then do that and short-circuit before the fixed.

                if (*(a + 1) != *(b + 1)) goto DiffOffset1;

                // Since we know that the first two chars are the same,
                // we can increment by 2 here and skip 4 bytes.
                // This leaves us 8-byte aligned, which results
                // on better perf for 64-bit platforms.
                length -= 2; a += 2; b += 2;

                // unroll the loop
#if BIT64
                while (length >= 12)
                {
                    if (*(long*)a != *(long*)b) goto DiffOffset0;
                    if (*(long*)(a + 4) != *(long*)(b + 4)) goto DiffOffset4;
                    if (*(long*)(a + 8) != *(long*)(b + 8)) goto DiffOffset8;
                    length -= 12; a += 12; b += 12;
                }
#else // BIT64
                while (length >= 10)
                {
                    if (*(int*)a != *(int*)b) goto DiffOffset0;
                    if (*(int*)(a + 2) != *(int*)(b + 2)) goto DiffOffset2;
                    if (*(int*)(a + 4) != *(int*)(b + 4)) goto DiffOffset4;
                    if (*(int*)(a + 6) != *(int*)(b + 6)) goto DiffOffset6;
                    if (*(int*)(a + 8) != *(int*)(b + 8)) goto DiffOffset8;
                    length -= 10; a += 10; b += 10; 
                }
#endif // BIT64

                // Fallback loop:
                // go back to slower code path and do comparison on 4 bytes at a time.
                // This depends on the fact that the String objects are
                // always zero terminated and that the terminating zero is not included
                // in the length. For odd string sizes, the last compare will include
                // the zero terminator.
                while (length > 0)
                {
                    if (*(int*)a != *(int*)b) goto DiffNextInt;
                    length -= 2;
                    a += 2;
                    b += 2;
                }

                // At this point, we have compared all the characters in at least one string.
                // The longer string will be larger.
                return strA.Length - strB.Length;

#if BIT64
            DiffOffset8: a += 4; b += 4;
            DiffOffset4: a += 4; b += 4;
#else // BIT64
                // Use jumps instead of falling through, since
                // otherwise going to DiffOffset8 will involve
                // 8 add instructions before getting to DiffNextInt
                DiffOffset8: a += 8; b += 8; goto DiffOffset0;
                DiffOffset6: a += 6; b += 6; goto DiffOffset0;
                DiffOffset4: a += 2; b += 2;
                DiffOffset2: a += 2; b += 2;
#endif // BIT64

            DiffOffset0:
                // If we reached here, we already see a difference in the unrolled loop above
#if BIT64
                if (*(int*)a == *(int*)b)
                {
                    a += 2; b += 2;
                }
#endif // BIT64

            DiffNextInt:
                if (*a != *b) return *a - *b;

                DiffOffset1:
                Debug.Assert(*(a + 1) != *(b + 1), "This char must be different if we reach here!");
                return *(a + 1) - *(b + 1);
            }
        }

        // Provides a culture-correct string comparison. StrA is compared to StrB
        // to determine whether it is lexicographically less, equal, or greater, and then returns
        // either a negative integer, 0, or a positive integer; respectively.
        //
        [Pure]
        public static int Compare(String strA, String strB)
        {
            return Compare(strA, strB, StringComparison.CurrentCulture);
        }


        // Provides a culture-correct string comparison. strA is compared to strB
        // to determine whether it is lexicographically less, equal, or greater, and then a
        // negative integer, 0, or a positive integer is returned; respectively.
        // The case-sensitive option is set by ignoreCase
        //
        [Pure]
        public static int Compare(String strA, String strB, bool ignoreCase)
        {
            var comparisonType = ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;
            return Compare(strA, strB, comparisonType);
        }


        // Provides a more flexible function for string comparision. See StringComparison 
        // for meaning of different comparisonType.
        [Pure]
        public static int Compare(String strA, String strB, StringComparison comparisonType)
        {
            // Single comparison to check if comparisonType is within [CurrentCulture .. OrdinalIgnoreCase]
            if ((uint)(comparisonType - StringComparison.CurrentCulture) > (uint)(StringComparison.OrdinalIgnoreCase - StringComparison.CurrentCulture))
            {
                throw new ArgumentException(SR.NotSupported_StringComparison, nameof(comparisonType));
            }
            Contract.EndContractBlock();

            if (object.ReferenceEquals(strA, strB))
            {
                return 0;
            }

            // They can't both be null at this point.
            if (strA == null)
            {
                return -1;
            }
            if (strB == null)
            {
                return 1;
            }

            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                    return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.None);

                case StringComparison.CurrentCultureIgnoreCase:
                    return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreCase);

                case StringComparison.InvariantCulture:
                    return CultureInfo.InvariantCulture.CompareInfo.Compare(strA, strB, CompareOptions.None);

                case StringComparison.InvariantCultureIgnoreCase:
                    return CultureInfo.InvariantCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreCase);

                case StringComparison.Ordinal:
                    // Most common case: first character is different.
                    // Returns false for empty strings.
                    if (strA._firstChar != strB._firstChar)
                    {
                        return strA._firstChar - strB._firstChar;
                    }

                    return CompareOrdinalHelper(strA, strB);

                case StringComparison.OrdinalIgnoreCase:
                    // If both strings are ASCII strings, we can take the fast path.
                    if (strA.IsAscii() && strB.IsAscii())
                    {
                        return (CompareOrdinalIgnoreCaseHelper(strA, strB));
                    }

                    return CompareInfo.CompareOrdinalIgnoreCase(strA, 0, strA.Length, strB, 0, strB.Length);

                default:
                    throw new NotSupportedException(SR.NotSupported_StringComparison);
            }
        }


        // Provides a culture-correct string comparison. strA is compared to strB
        // to determine whether it is lexicographically less, equal, or greater, and then a
        // negative integer, 0, or a positive integer is returned; respectively.
        //
        [Pure]
        public static int Compare(String strA, String strB, CultureInfo culture, CompareOptions options)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }
            Contract.EndContractBlock();

            return culture.CompareInfo.Compare(strA, strB, options);
        }



        // Provides a culture-correct string comparison. strA is compared to strB
        // to determine whether it is lexicographically less, equal, or greater, and then a
        // negative integer, 0, or a positive integer is returned; respectively.
        // The case-sensitive option is set by ignoreCase, and the culture is set
        // by culture
        //
        [Pure]
        public static int Compare(String strA, String strB, bool ignoreCase, CultureInfo culture)
        {
            var options = ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None;
            return Compare(strA, strB, culture, options);
        }

        // Determines whether two string regions match.  The substring of strA beginning
        // at indexA of length count is compared with the substring of strB
        // beginning at indexB of the same length.
        //
        [Pure]
        public static int Compare(String strA, int indexA, String strB, int indexB, int length)
        {
            // NOTE: It's important we call the boolean overload, and not the StringComparison
            // one. The two have some subtly different behavior (see notes in the former).
            return Compare(strA, indexA, strB, indexB, length, ignoreCase: false);
        }

        // Determines whether two string regions match.  The substring of strA beginning
        // at indexA of length count is compared with the substring of strB
        // beginning at indexB of the same length.  Case sensitivity is determined by the ignoreCase boolean.
        //
        [Pure]
        public static int Compare(String strA, int indexA, String strB, int indexB, int length, bool ignoreCase)
        {
            // Ideally we would just forward to the string.Compare overload that takes
            // a StringComparison parameter, and just pass in CurrentCulture/CurrentCultureIgnoreCase.
            // That function will return early if an optimization can be applied, e.g. if
            // (object)strA == strB && indexA == indexB then it will return 0 straightaway.
            // There are a couple of subtle behavior differences that prevent us from doing so
            // however:
            // - string.Compare(null, -1, null, -1, -1, StringComparison.CurrentCulture) works
            //   since that method also returns early for nulls before validation. It shouldn't
            //   for this overload.
            // - Since we originally forwarded to CompareInfo.Compare for all of the argument
            //   validation logic, the ArgumentOutOfRangeExceptions thrown will contain different
            //   parameter names.
            // Therefore, we have to duplicate some of the logic here.

            int lengthA = length;
            int lengthB = length;

            if (strA != null)
            {
                lengthA = Math.Min(lengthA, strA.Length - indexA);
            }

            if (strB != null)
            {
                lengthB = Math.Min(lengthB, strB.Length - indexB);
            }

            var options = ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None;
            return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, indexA, lengthA, strB, indexB, lengthB, options);
        }

        // Determines whether two string regions match.  The substring of strA beginning
        // at indexA of length length is compared with the substring of strB
        // beginning at indexB of the same length.  Case sensitivity is determined by the ignoreCase boolean,
        // and the culture is set by culture.
        //
        [Pure]
        public static int Compare(String strA, int indexA, String strB, int indexB, int length, bool ignoreCase, CultureInfo culture)
        {
            var options = ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None;
            return Compare(strA, indexA, strB, indexB, length, culture, options);
        }


        // Determines whether two string regions match.  The substring of strA beginning
        // at indexA of length length is compared with the substring of strB
        // beginning at indexB of the same length.
        //
        [Pure]
        public static int Compare(String strA, int indexA, String strB, int indexB, int length, CultureInfo culture, CompareOptions options)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }
            Contract.EndContractBlock();

            int lengthA = length;
            int lengthB = length;

            if (strA != null)
            {
                lengthA = Math.Min(lengthA, strA.Length - indexA);
            }

            if (strB != null)
            {
                lengthB = Math.Min(lengthB, strB.Length - indexB);
            }

            return culture.CompareInfo.Compare(strA, indexA, lengthA, strB, indexB, lengthB, options);
        }

        [Pure]
        public static int Compare(String strA, int indexA, String strB, int indexB, int length, StringComparison comparisonType)
        {
            if (comparisonType < StringComparison.CurrentCulture || comparisonType > StringComparison.OrdinalIgnoreCase)
            {
                throw new ArgumentException(SR.NotSupported_StringComparison, nameof(comparisonType));
            }
            Contract.EndContractBlock();

            if (strA == null || strB == null)
            {
                if (object.ReferenceEquals(strA, strB))
                {
                    // They're both null
                    return 0;
                }

                return strA == null ? -1 : 1;
            }

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), SR.ArgumentOutOfRange_NegativeLength);
            }

            if (indexA < 0 || indexB < 0)
            {
                string paramName = indexA < 0 ? nameof(indexA) : nameof(indexB);
                throw new ArgumentOutOfRangeException(paramName, SR.ArgumentOutOfRange_Index);
            }

            if (strA.Length - indexA < 0 || strB.Length - indexB < 0)
            {
                string paramName = strA.Length - indexA < 0 ? nameof(indexA) : nameof(indexB);
                throw new ArgumentOutOfRangeException(paramName, SR.ArgumentOutOfRange_Index);
            }

            if (length == 0 || (object.ReferenceEquals(strA, strB) && indexA == indexB))
            {
                return 0;
            }

            int lengthA = Math.Min(length, strA.Length - indexA);
            int lengthB = Math.Min(length, strB.Length - indexB);

            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                    return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, indexA, lengthA, strB, indexB, lengthB, CompareOptions.None);

                case StringComparison.CurrentCultureIgnoreCase:
                    return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, indexA, lengthA, strB, indexB, lengthB, CompareOptions.IgnoreCase);

                case StringComparison.InvariantCulture:
                    return CultureInfo.InvariantCulture.CompareInfo.Compare(strA, indexA, lengthA, strB, indexB, lengthB, CompareOptions.None);

                case StringComparison.InvariantCultureIgnoreCase:
                    return CultureInfo.InvariantCulture.CompareInfo.Compare(strA, indexA, lengthA, strB, indexB, lengthB, CompareOptions.IgnoreCase);

                case StringComparison.Ordinal:
                    return CompareOrdinalHelper(strA, indexA, lengthA, strB, indexB, lengthB);

                case StringComparison.OrdinalIgnoreCase:
                    return (CompareInfo.CompareOrdinalIgnoreCase(strA, indexA, lengthA, strB, indexB, lengthB));

                default:
                    throw new ArgumentException(SR.NotSupported_StringComparison);
            }
        }

        // Compares strA and strB using an ordinal (code-point) comparison.
        //
        [Pure]
        public static int CompareOrdinal(String strA, String strB)
        {
            if (object.ReferenceEquals(strA, strB))
            {
                return 0;
            }

            // They can't both be null at this point.
            if (strA == null)
            {
                return -1;
            }
            if (strB == null)
            {
                return 1;
            }

            // Most common case, first character is different.
            // This will return false for empty strings.
            if (strA._firstChar != strB._firstChar)
            {
                return strA._firstChar - strB._firstChar;
            }

            return CompareOrdinalHelper(strA, strB);
        }


        // Compares strA and strB using an ordinal (code-point) comparison.
        //
        [Pure]
        public static int CompareOrdinal(String strA, int indexA, String strB, int indexB, int length)
        {
            if (strA == null || strB == null)
            {
                if (object.ReferenceEquals(strA, strB))
                {
                    // They're both null
                    return 0;
                }

                return strA == null ? -1 : 1;
            }

            // COMPAT: Checking for nulls should become before the arguments are validated,
            // but other optimizations which allow us to return early should come after.

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), SR.ArgumentOutOfRange_NegativeCount);
            }

            if (indexA < 0 || indexB < 0)
            {
                string paramName = indexA < 0 ? nameof(indexA) : nameof(indexB);
                throw new ArgumentOutOfRangeException(paramName, SR.ArgumentOutOfRange_Index);
            }

            int lengthA = Math.Min(length, strA.Length - indexA);
            int lengthB = Math.Min(length, strB.Length - indexB);

            if (lengthA < 0 || lengthB < 0)
            {
                string paramName = lengthA < 0 ? nameof(indexA) : nameof(indexB);
                throw new ArgumentOutOfRangeException(paramName, SR.ArgumentOutOfRange_Index);
            }

            if (length == 0 || (object.ReferenceEquals(strA, strB) && indexA == indexB))
            {
                return 0;
            }

            return CompareOrdinalHelper(strA, indexA, lengthA, strB, indexB, lengthB);
        }

        // Compares this String to another String (cast as object), returning an integer that
        // indicates the relationship. This method returns a value less than 0 if this is less than value, 0
        // if this is equal to value, or a value greater than 0 if this is greater than value.
        //
        [Pure]
        public int CompareTo(Object value)
        {
            if (value == null)
            {
                return 1;
            }

            string other = value as string;

            if (other == null)
            {
                throw new ArgumentException(SR.Arg_MustBeString);
            }

            return CompareTo(other); // will call the string-based overload
        }

        // Determines the sorting relation of StrB to the current instance.
        //
        [Pure]
        public int CompareTo(String strB)
        {
            return string.Compare(this, strB, StringComparison.CurrentCulture);
        }

        // Determines whether a specified string is a suffix of the current instance.
        //
        // The case-sensitive and culture-sensitive option is set by options,
        // and the default culture is used.
        //        
        [Pure]
        public Boolean EndsWith(String value)
        {
            return EndsWith(value, StringComparison.CurrentCulture);
        }

        [Pure]
        public Boolean EndsWith(String value, StringComparison comparisonType)
        {
            if ((Object)value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (comparisonType < StringComparison.CurrentCulture || comparisonType > StringComparison.OrdinalIgnoreCase)
            {
                throw new ArgumentException(SR.NotSupported_StringComparison, nameof(comparisonType));
            }
            Contract.EndContractBlock();

            if ((Object)this == (Object)value)
            {
                return true;
            }

            if (value.Length == 0)
            {
                return true;
            }

            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                    return CultureInfo.CurrentCulture.CompareInfo.IsSuffix(this, value, CompareOptions.None);

                case StringComparison.CurrentCultureIgnoreCase:
                    return CultureInfo.CurrentCulture.CompareInfo.IsSuffix(this, value, CompareOptions.IgnoreCase);

                case StringComparison.InvariantCulture:
                    return CultureInfo.InvariantCulture.CompareInfo.IsSuffix(this, value, CompareOptions.None);

                case StringComparison.InvariantCultureIgnoreCase:
                    return CultureInfo.InvariantCulture.CompareInfo.IsSuffix(this, value, CompareOptions.IgnoreCase);

                case StringComparison.Ordinal:
                    return this.Length < value.Length ? false : (CompareOrdinalHelper(this, this.Length - value.Length, value.Length, value, 0, value.Length) == 0);

                case StringComparison.OrdinalIgnoreCase:
                    return this.Length < value.Length ? false : (CompareInfo.CompareOrdinalIgnoreCase(this, this.Length - value.Length, value.Length, value, 0, value.Length) == 0);
                default:
                    throw new ArgumentException(SR.NotSupported_StringComparison, nameof(comparisonType));
            }
        }

        [Pure]
        public Boolean EndsWith(String value, Boolean ignoreCase, CultureInfo culture)
        {
            if (null == value)
            {
                throw new ArgumentNullException(nameof(value));
            }
            Contract.EndContractBlock();

            if ((object)this == (object)value)
            {
                return true;
            }

            CultureInfo referenceCulture;
            if (culture == null)
                referenceCulture = CultureInfo.CurrentCulture;
            else
                referenceCulture = culture;

            return referenceCulture.CompareInfo.IsSuffix(this, value, ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None);
        }

        [Pure]
        public bool EndsWith(char value)
        {
            int thisLen = Length;
            return thisLen != 0 && this[thisLen - 1] == value;
        }

        // Determines whether two strings match.
        public override bool Equals(Object obj)
        {
            if (object.ReferenceEquals(this, obj))
                return true;

            string str = obj as string;
            if (str == null)
                return false;

            if (this.Length != str.Length)
                return false;

            return EqualsHelper(this, str);
        }

        // Determines whether two strings match.
        [Pure]
        public bool Equals(String value)
        {
            if (object.ReferenceEquals(this, value))
                return true;

            // NOTE: No need to worry about casting to object here.
            // If either side of an == comparison between strings
            // is null, Roslyn generates a simple ceq instruction
            // instead of calling string.op_Equality.
            if (value == null)
                return false;

            if (this.Length != value.Length)
                return false;

            return EqualsHelper(this, value);
        }

        [Pure]
        public bool Equals(String value, StringComparison comparisonType)
        {
            if (comparisonType < StringComparison.CurrentCulture || comparisonType > StringComparison.OrdinalIgnoreCase)
                throw new ArgumentException(SR.NotSupported_StringComparison, nameof(comparisonType));
            Contract.EndContractBlock();

            if ((Object)this == (Object)value)
            {
                return true;
            }

            if ((Object)value == null)
            {
                return false;
            }

            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                    return (CultureInfo.CurrentCulture.CompareInfo.Compare(this, value, CompareOptions.None) == 0);

                case StringComparison.CurrentCultureIgnoreCase:
                    return (CultureInfo.CurrentCulture.CompareInfo.Compare(this, value, CompareOptions.IgnoreCase) == 0);

                case StringComparison.InvariantCulture:
                    return (CultureInfo.InvariantCulture.CompareInfo.Compare(this, value, CompareOptions.None) == 0);

                case StringComparison.InvariantCultureIgnoreCase:
                    return (CultureInfo.InvariantCulture.CompareInfo.Compare(this, value, CompareOptions.IgnoreCase) == 0);

                case StringComparison.Ordinal:
                    if (this.Length != value.Length)
                        return false;
                    return EqualsHelper(this, value);

                case StringComparison.OrdinalIgnoreCase:
                    if (this.Length != value.Length)
                        return false;

                    // If both strings are ASCII strings, we can take the fast path.
                    if (this.IsAscii() && value.IsAscii())
                    {
                        return EqualsIgnoreCaseAsciiHelper(this, value);
                    }

                    return (CompareInfo.CompareOrdinalIgnoreCase(this, 0, this.Length, value, 0, value.Length) == 0);

                default:
                    throw new ArgumentException(SR.NotSupported_StringComparison, nameof(comparisonType));
            }
        }


        // Determines whether two Strings match.
        [Pure]
        public static bool Equals(String a, String b)
        {
            if ((Object)a == (Object)b)
            {
                return true;
            }

            if ((Object)a == null || (Object)b == null || a.Length != b.Length)
            {
                return false;
            }

            return EqualsHelper(a, b);
        }

        [Pure]
        public static bool Equals(String a, String b, StringComparison comparisonType)
        {
            if (comparisonType < StringComparison.CurrentCulture || comparisonType > StringComparison.OrdinalIgnoreCase)
                throw new ArgumentException(SR.NotSupported_StringComparison, nameof(comparisonType));
            Contract.EndContractBlock();

            if ((Object)a == (Object)b)
            {
                return true;
            }

            if ((Object)a == null || (Object)b == null)
            {
                return false;
            }

            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                    return (CultureInfo.CurrentCulture.CompareInfo.Compare(a, b, CompareOptions.None) == 0);

                case StringComparison.CurrentCultureIgnoreCase:
                    return (CultureInfo.CurrentCulture.CompareInfo.Compare(a, b, CompareOptions.IgnoreCase) == 0);

                case StringComparison.InvariantCulture:
                    return (CultureInfo.InvariantCulture.CompareInfo.Compare(a, b, CompareOptions.None) == 0);

                case StringComparison.InvariantCultureIgnoreCase:
                    return (CultureInfo.InvariantCulture.CompareInfo.Compare(a, b, CompareOptions.IgnoreCase) == 0);

                case StringComparison.Ordinal:
                    if (a.Length != b.Length)
                        return false;

                    return EqualsHelper(a, b);

                case StringComparison.OrdinalIgnoreCase:
                    if (a.Length != b.Length)
                        return false;
                    else
                    {
                        // If both strings are ASCII strings, we can take the fast path.
                        if (a.IsAscii() && b.IsAscii())
                        {
                            return EqualsIgnoreCaseAsciiHelper(a, b);
                        }
                        // Take the slow path.

                        return (CompareInfo.CompareOrdinalIgnoreCase(a, 0, a.Length, b, 0, b.Length) == 0);
                    }

                default:
                    throw new ArgumentException(SR.NotSupported_StringComparison, nameof(comparisonType));
            }
        }

        public static bool operator ==(String a, String b)
        {
            return String.Equals(a, b);
        }

        public static bool operator !=(String a, String b)
        {
            return !String.Equals(a, b);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern int InternalMarvin32HashString(string s);

        // Gets a hash code for this string.  If strings A and B are such that A.Equals(B), then
        // they will return the same hash code.
        public override int GetHashCode()
        {
            return InternalMarvin32HashString(this);
        }

        // Gets a hash code for this string and this comparison. If strings A and B and comparition C are such
        // that String.Equals(A, B, C), then they will return the same hash code with this comparison C.
        public int GetHashCode(StringComparison comparisonType) => StringComparer.FromComparison(comparisonType).GetHashCode(this);

        // Use this if and only if you need the hashcode to not change across app domains (e.g. you have an app domain agile
        // hash table).
        internal int GetLegacyNonRandomizedHashCode()
        {
            unsafe
            {
                fixed (char* src = &_firstChar)
                {
                    Debug.Assert(src[this.Length] == '\0', "src[this.Length] == '\\0'");
                    Debug.Assert(((int)src) % 4 == 0, "Managed string should start at 4 bytes boundary");
#if BIT64
                    int hash1 = 5381;
#else // !BIT64 (32)
                    int hash1 = (5381<<16) + 5381;
#endif
                    int hash2 = hash1;

#if BIT64
                    int c;
                    char* s = src;
                    while ((c = s[0]) != 0)
                    {
                        hash1 = ((hash1 << 5) + hash1) ^ c;
                        c = s[1];
                        if (c == 0)
                            break;
                        hash2 = ((hash2 << 5) + hash2) ^ c;
                        s += 2;
                    }
#else // !BIT64 (32)
                    // 32 bit machines.
                    int* pint = (int *)src;
                    int len = this.Length;
                    while (len > 2)
                    {
                        hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ pint[0];
                        hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ pint[1];
                        pint += 2;
                        len  -= 4;
                    }

                    if (len > 0)
                    {
                        hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ pint[0];
                    }
#endif
#if DEBUG
                    // We want to ensure we can change our hash function daily.
                    // This is perfectly fine as long as you don't persist the
                    // value from GetHashCode to disk or count on String A 
                    // hashing before string B.  Those are bugs in your code.
                    hash1 ^= ThisAssembly.DailyBuildNumber;
#endif
                    return hash1 + (hash2 * 1566083941);
                }
            }
        }

        // Determines whether a specified string is a prefix of the current instance
        //
        [Pure]
        public Boolean StartsWith(String value)
        {
            if ((Object)value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            Contract.EndContractBlock();
            return StartsWith(value, StringComparison.CurrentCulture);
        }

        [Pure]
        public Boolean StartsWith(String value, StringComparison comparisonType)
        {
            if ((Object)value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (comparisonType < StringComparison.CurrentCulture || comparisonType > StringComparison.OrdinalIgnoreCase)
            {
                throw new ArgumentException(SR.NotSupported_StringComparison, nameof(comparisonType));
            }
            Contract.EndContractBlock();

            if ((Object)this == (Object)value)
            {
                return true;
            }

            if (value.Length == 0)
            {
                return true;
            }

            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                    return CultureInfo.CurrentCulture.CompareInfo.IsPrefix(this, value, CompareOptions.None);

                case StringComparison.CurrentCultureIgnoreCase:
                    return CultureInfo.CurrentCulture.CompareInfo.IsPrefix(this, value, CompareOptions.IgnoreCase);

                case StringComparison.InvariantCulture:
                    return CultureInfo.InvariantCulture.CompareInfo.IsPrefix(this, value, CompareOptions.None);

                case StringComparison.InvariantCultureIgnoreCase:
                    return CultureInfo.InvariantCulture.CompareInfo.IsPrefix(this, value, CompareOptions.IgnoreCase);

                case StringComparison.Ordinal:
                    if (this.Length < value.Length || _firstChar != value._firstChar)
                    {
                        return false;
                    }
                    return (value.Length == 1) ?
                            true :                 // First char is the same and thats all there is to compare
                            StartsWithOrdinalHelper(this, value);

                case StringComparison.OrdinalIgnoreCase:
                    if (this.Length < value.Length)
                    {
                        return false;
                    }

                    return (CompareInfo.CompareOrdinalIgnoreCase(this, 0, value.Length, value, 0, value.Length) == 0);

                default:
                    throw new ArgumentException(SR.NotSupported_StringComparison, nameof(comparisonType));
            }
        }

        [Pure]
        public Boolean StartsWith(String value, Boolean ignoreCase, CultureInfo culture)
        {
            if (null == value)
            {
                throw new ArgumentNullException(nameof(value));
            }
            Contract.EndContractBlock();

            if ((object)this == (object)value)
            {
                return true;
            }

            CultureInfo referenceCulture;
            if (culture == null)
                referenceCulture = CultureInfo.CurrentCulture;
            else
                referenceCulture = culture;

            return referenceCulture.CompareInfo.IsPrefix(this, value, ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None);
        }

        [Pure]
        public bool StartsWith(char value) => Length != 0 && _firstChar == value;
    }
}
