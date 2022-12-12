using UnityEngine;
using System;

[Serializable]
public class BinaryTree
{
    // to keep track of the whole tree,
    // only keep a reference to the root node
    // since it keeps references to child nodes
    public BinaryNode rootNode;

    // to iterate through tree, keep track of current node and query count
    internal BinaryNode currentNode;

    // query count, keep track of current
    internal int currentQuery;

    string listOfNodes;

    /// <summary>
    /// Constructor for new empty tree
    /// </summary>
    public BinaryTree() {
        
    }

    /// <summary>
    /// Constructor for new tree with parameters for the first node
    /// </summary>
    /// <param name="text">Question at root node</param>
    /// <param name="leftNodeText">Yes answer at root node</param>
    /// <param name="rightNodeText">No answer at root node</param>
    public BinaryTree(string text, string leftNodeText, string rightNodeText)
    {
        // create root node with question
        rootNode = new BinaryNode(text);

        // set l and r text as child nodes to the root
        rootNode.rightNode = new BinaryNode(leftNodeText);
        rootNode.leftNode = new BinaryNode(rightNodeText);

        Debug.Log(PrintPreOrder(rootNode));
    }

    /// <summary>
    /// Sets up to start iterating. 
    /// </summary>
    public void StartIterating()
    {
        // set the query count and current node,
        // which is the first when starting query
        currentQuery = 1;
        currentNode = rootNode;
    }

    /// <summary>
    /// Iterates to the next node. 
    /// </summary>
    /// <param name="left">Whether to iterate to the left or right</param>
    public void IterateNext(bool left)
    {
        // cannot query next if not properly started
        if (currentNode == null || currentQuery <= 0)
        {
            Debug.LogError("Cannot iterate to next node, call StartQuery first!");
            return;
        }

        // cannot query next if this is a leaf node
        if (currentNode.IsLeafNode())
        {
            Debug.LogError("Cannot iterate to next node, this is a leaf node.");
            return;
        }

        // increase query
        currentQuery++;

        // get either left or right node as the new current
        if (left)
            currentNode = currentNode.leftNode;
        else
            currentNode = currentNode.rightNode;
    }

    /// <summary>
    /// Resets the current query.
    /// </summary>
    public void EndIterating()
    {
        // reset node and count
        currentQuery = -1;
        currentNode = null;
    }

    
    public string PrintPreOrder(BinaryNode node)
    {
        if (node == null)
            return listOfNodes;
        /* then print the data of node */
        //Console.Write(node.text + " ");
        listOfNodes += node.text + ", ";

        /* first recur on left child */
        PrintPreOrder(node.leftNode);

        /* now recur on right child */
        PrintPreOrder(node.rightNode);
        return listOfNodes;
    }

    public string PrintPostOrder(BinaryNode node)
    {
        if (node == null)
            return listOfNodes;

        /* first recur on left child */
        PrintPostOrder(node.leftNode);


        /* now recur on right child */
        PrintPostOrder(node.rightNode);

        /* then print the data of node */
        //Console.Write(node.text + " ");
        listOfNodes += node.text + ", ";

        return listOfNodes;
    }
    public string PrintInOrder(BinaryNode node)
    {
        if (node == null)
            return listOfNodes;

        /* first recur on left child */
        PrintInOrder(node.leftNode);

        /* then print the data of node */
        //Console.Write(node.text + " ");
        listOfNodes += node.text + ", ";
        /* now recur on right child */
        PrintInOrder(node.rightNode);
        return listOfNodes;
    }

    public void ClearListOfNodes()
    {
        listOfNodes = "";
    }
}