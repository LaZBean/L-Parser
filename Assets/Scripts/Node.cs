using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Node
{
    public string name;
    public Node left;
    public Node right;

    public Node(string name = "node", Node left = null, Node right = null)
    {
        this.name = name;
        this.left = left;
        this.right = right;
    }
}
