using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parser : MonoBehaviour {

    public static Parser i;

    void Awake()
    {
        i = this;
    }

    

    int errorCount;


    public string input;
    public int charIndx = 0;

    public string bufferStr;

    Dictionary<string, int> variablesTable = new Dictionary<string, int>(); // Таблица переменных.

    
    


    Token curToken = new Token(";", TokenType.PRINT);    // Хранит последний возврат функции GetToken().
    public List<Token> tokens;  //список токенов

    Node curNode;
    public List<Node> nodes;




  






    








    //Начало парсинга
    public void Compile(string input)
    {

        variablesTable.Clear();
        tokens.Clear();
        nodes.Clear();

        errorCount = 0;

        this.input = input;
        charIndx = -1;


        while (charIndx < input.Length)
        {

            GetToken();

            if (curToken.type == TokenType.END)
            {
                break;
            }

            if (curToken.type == TokenType.PRINT)
            {
                
                continue;
            }

            curNode = new Node();
            nodes.Add(curNode);
            expr(false, curNode);
        }
        

        Computer.i.OutTokenList(tokens.ToArray());
        Computer.i.OutTree(nodes);



        Computer.i.OutMessage("[Compilation finished!] \tErrors:[" + errorCount + "]", Computer.OutMsgType.warning);

        Computer.i.OutMessage("[" + variablesTable.Count + "] variables", Computer.OutMsgType.warning);
        foreach (string key in variablesTable.Keys)
        {
            string v = (variablesTable[key] == 0 || variablesTable[key] == 1) ? variablesTable[key] + "" : "undefined";
            Computer.i.OutMessage("\t[" + key + "] = " + v + "", Computer.OutMsgType.message);
        }
    }






    Token AddToken(string v, TokenType type)
    {
        Token t = new Token(v, type);
        tokens.Add(t);
        curToken = t;
        return t;
    }


    Token GetToken()
    {
        char ch;
        bufferStr = "";

        do
        {   //Пропустить все пробелы


            charIndx++;


            if (charIndx >= input.Length)
            {
                return curToken = new Token("", TokenType.END); // Завершить работу.
            }

            ch = input[charIndx];



        } while (ch == ' ' || ch == '\n');





        if (ch == ';' || ch == '(' || ch == ')')
        {       // ; ( )
            bufferStr += ch;
            return AddToken(bufferStr, (TokenType)ch);
        }
        else if (ch == ':' && nextCharIs('=', 1))
        {   //:=
            bufferStr += ":=";
            charIndx += 1;
            return AddToken(bufferStr, TokenType.ASSIGN);
        }
        else if (ch == '0' || ch == '1')
        {   //0 , 1
            bufferStr += ch;
            return AddToken(bufferStr, TokenType.CONST);
        }
        else if (ch == 'x' && nextCharIs('o', 1) && nextCharIs('r', 2))
        {   //XOR
            bufferStr += "xor";
            charIndx += 2;
            return AddToken(bufferStr, TokenType.XOR);
        }
        else if (ch == 'o' && nextCharIs('r', 1))
        {   //OR
            bufferStr += "or";
            charIndx += 1;
            return AddToken(bufferStr, TokenType.OR);
        }
        else if (ch == 'a' && nextCharIs('n', 1) && nextCharIs('d', 2))
        {   //AND
            bufferStr += "and";
            charIndx += 2;
            return AddToken(bufferStr, TokenType.AND);
        }
        else if (ch == 'n' && nextCharIs('o', 1) && nextCharIs('t', 2))
        {   //NOT
            bufferStr += "not";
            charIndx += 2;
            return AddToken(bufferStr, TokenType.NOT);
        }
        else
        {

            if (System.Char.IsLetter(ch))
            {       //NAME

                while (charIndx < input.Length)
                {
                    ch = input[charIndx];

                    if (System.Char.IsLetter(ch) || System.Char.IsNumber(ch))
                    {
                        bufferStr += ch;
                        charIndx++;
                    }
                    else
                    {
                        charIndx--;
                        break;
                    }
                }

                if(!variablesTable.ContainsKey(bufferStr))
                    variablesTable[bufferStr] = -1;

                return AddToken(bufferStr, TokenType.NAME);
            }
        }



        Error("Bad Token [" + ch + "]");
        return AddToken(";", TokenType.PRINT);
    }



    bool nextCharIs(char c, int step)
    {
        int i = charIndx + step;
        if (i < 0 || i >= input.Length || input[i] != c)
        {
            return false;
        }
        return true;
    }

    char nextChar(int step)
    {
        int i = charIndx + step;
        if (i < 0 || i >= input.Length)
        {
            return ' ';
        }
        return input[i];
    }







    
    //Priority
    //  NOT
    //  AND
    //  OR, XOR

    //prim() - обрабатывает первичные выражения. приоритет 1. ПРИМИТИВ

    int prim(bool getNext, Node n)
    {
        n.name = "[PRIM]";
        if (getNext)
            GetToken();


        if (curToken.type == TokenType.CONST)
        {
            int v = int.Parse(curToken.v);
            GetToken();

            n.name = v + "";

            return v;
        }
        if (curToken.type == TokenType.NAME)
        {

            int v = variablesTable[curToken.v];
            Token t = curToken;

            n.name = curToken.v;

            if (GetToken().type == TokenType.ASSIGN)
            {
                n.left = new Node(":=");    //
                n.left.left = new Node();   //

                v = expr(true, n.left.left);
                variablesTable[t.v] = v;
            }

            if (variablesTable[t.v] == -1)
                return Error("Undefined variable ["+t.v+"]");

           
            return v;
        }
        if (curToken.type == TokenType.NOT)
        {
            n.name = ("NOT");  //
            n.left = new Node();

            int v = prim(true, n.left);
            v = System.Convert.ToInt32(v != 1);
            
            return v;
        }
        if (curToken.type == TokenType.LB)
        {
            n.name = ("()");  //
            n.left = new Node();   //

            int v = expr(true, n.left);
            if (curToken.type != TokenType.RB)
            {
                return Error("')' expected");
            }
            GetToken();
            
            return v;
        }

        

        return Error("Primary expected!");
    }

    // term() - операции приоритет 2. ПОДВЫРАЖЕНИЯ
    int term(bool getNext, Node n)
    {
        n.name = "[TERM]";
        Node nodeLeft = new Node();   //

        int left = prim(getNext, nodeLeft);

        

        if (curToken.type == TokenType.AND)
        {
            n.name = ("AND");    //
            n.right = new Node();

            int right = prim(true, n.right);
            bool b = (left == right) && (left == 1);
            left = System.Convert.ToInt32(b);
        }

        n.left = nodeLeft;


        return left;
    }


    // expr() - операции приоритет 3. ВЫРАЖЕНИЯ
    int expr(bool getNext, Node n)
    {
        n.name = "[EXPR]";
        Node nodeLeft = new Node();

        int left = term(getNext, nodeLeft);


        if (curToken.type == TokenType.OR)
        {
            n.name = ("OR");    //
            n.right = new Node();   //

            int right = term(true, n.right);
            bool b = (left == 1) || (right == 1);
            left = System.Convert.ToInt32(b);
        }
        if (curToken.type == TokenType.XOR)
        {
            n.name = ("XOR");    //
            n.right = new Node();   //

            int right = term(true, n.right);
            bool b = (left == 1 && right == 0) || (right == 1 && left == 0);
            left = System.Convert.ToInt32(b);
        }

        n.left = nodeLeft;   //

        return left;
    }












    //ERROR
    int Error(string error_message)
    {
        errorCount++;
        Computer.i.OutMessage("Error: " + error_message + "", Computer.OutMsgType.error);
        return -1;
    }
}
