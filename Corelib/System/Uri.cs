namespace System;

public class Uri
{
    
    //
    // IsHexDigit
    //
    //  Determines whether a character is a valid hexadecimal digit in the range
    //  [0..9] | [A..F] | [a..f]
    //
    // Inputs:
    //  <argument>  character
    //      Character to test
    //
    // Returns:
    //  true if <character> is a hexadecimal digit character
    //
    // Throws:
    //  Nothing
    //
    public static bool IsHexDigit(char character)
    {
        return HexConverter.IsHexChar(character);
    }
    
    //
    // Returns:
    //  Number in the range 0..15
    //
    // Throws:
    //  ArgumentException
    //
    public static int FromHex(char digit)
    {
        int result = HexConverter.FromChar(digit);
        if (result == 0xFF)
        {
            throw new ArgumentException(null, nameof(digit));
        }

        return result;
    }

}