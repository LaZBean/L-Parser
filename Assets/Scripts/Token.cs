using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Token
{
    public string v;
    public TokenType type;

    public Token(string v, TokenType type)
    {
        this.v = v;
        this.type = type;
    }
}

[System.Serializable]
public enum TokenType
{
    NAME, CONST, END,
    XOR, OR, AND, NOT,
    ASSIGN,
    PRINT = ';', LB = '(', RB = ')',
}
