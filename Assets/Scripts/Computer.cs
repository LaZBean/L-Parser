using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Computer : MonoBehaviour {

	public static Computer i;

	public InputField inputField;

	public Text outputText;
	public Text outListText;
    public Text outTreeText;




	public enum OutMsgType
	{
		message = 0,
		error = 1,
		warning = 2,
	}

    
	void Awake(){
		i = this;
	}




    //Метод запускает анализатор
	public void CompileScript(){
        ClearOutput();

        Parser.i.Compile(inputField.text);
    }


    


	//Метод выводит сообщение в консоль вывода
	public void OutMessage(string message, OutMsgType msgType=OutMsgType.message){

		if (msgType == OutMsgType.error)
			message = "<color=red>" + message + "</color>";
		else if(msgType == OutMsgType.warning)
			message = "<color=yellow>" + message + "</color>";

		outputText.text += message + "\n";
	}

    //Метод отображает список токенов
    public void OutTokenList(Token[] tokens)
    {
        outListText.text = "";
        foreach (Token token in tokens)
        {
            outListText.text += "" + token.type + " - [" + (token.v) + "]\n";
        }
    }

    //Метод выводит деревья разбора
    public void OutTree(List<Node> nodes)
    {
        outTreeText.text = "";

        for (int i = 0; i < nodes.Count; i++)
        {
            OutNode(nodes[i], 0);
            outTreeText.text += "\n===================================================================================\n\n";
        }
    }

    //Метод рекурсивно выводит все узлы дерева
    public void OutNode(Node node, int lvl)
    {
        if (node == null) return;

        int i = 0;
        while (i < lvl)
        {
            outTreeText.text += "\t";
            i++;
        }

        if (node.name[0] == '[') {
            outTreeText.text += "<color=yellow>" + node.name + "</color>\n";
        }
        else
            outTreeText.text += node.name + "\n";

        OutNode(node.left, lvl + 1);
        OutNode(node.right,lvl + 1);

    }




    //Метод очищает консоль вывода
    public void ClearOutput()
    {
        outputText.text = "";
    }
}
