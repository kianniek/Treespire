using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class BinaryNode 
{
    // the text of this node
    public string text;

    // references to left and right node
    public BinaryNode leftNode;
    public BinaryNode rightNode;

    /// <summary>
    /// Constructor for new empty node.
    /// </summary>
    public BinaryNode() { }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="text">The text (content) of this node</param>
    internal BinaryNode(string text) 
    {
        // set the message
        this.text = text;

        // set nodes initially to null
        leftNode = null;
        rightNode = null;
    }

    /// <summary>
    /// Whether this node is a leaf node.
    /// </summary>
    /// <returns>True is this is a leaf node</returns>
    internal bool IsLeafNode()
    {
        // if we don't have nodes, we're a leaf node 
        if (leftNode == null && rightNode == null)
            return true;
        else
            return false;
    }
}