using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node <T>
{
    // the content of this node
    public T content;

    // references to childeren
    public Node<T>[] childNodes;

    /// <summary>
    /// Constructor for new empty node
    /// </summary>
    public Node() { }

    /// <summary>
    /// Copy-constructor 
    /// </summary>
    /// <param name="node">The node to copy this node from</param>
    internal Node(Node<T> node)
    {
        // get the text
        content = node.content;

        // make clones of all childeren
        if (node.childNodes != null)
        {
            childNodes = new Node<T>[node.childNodes.Length];
            for (int i = 0; i < node.childNodes.Length; i++)
                childNodes[i] = new Node<T>(node.childNodes[i]);
        }
        else
            childNodes = null;
    }

    /// <summary>
    /// Constructor for node without children.
    /// </summary>
    /// <param name="content">The content of this node</param>
    internal Node(T content) 
    {
        // set the message
        this.content = content;

        // set child nodes to null initially
        childNodes = null;
    }

    /// <summary>
    /// Constructor for node with content from which child nodes are created.
    /// </summary>
    /// <param name="content">The content of this node</param>
    /// <param name="childNodes">The content of the child nodes of this node</param>
    internal Node(T content, T[] childNodes)
    {
        // set the message
        this.content = content;

        // set child nodes 
        this.childNodes = new Node<T>[childNodes.Length];
        for (int i = 0; i < childNodes.Length; i++)
        {
            this.childNodes[i] = new Node<T>(childNodes[i]);
        }
    }

    /// <summary>
    /// Constructor for node with children as nodes
    /// </summary>
    /// <param name="content">The content of this node</param>
    /// <param name="childNodes">The child nodes of this node</param>
    internal Node(T content, Node<T>[] childNodes)
    {
        // set the message
        this.content = content;

        // set child nodes 
        this.childNodes = new Node<T>[childNodes.Length];
        for (int i = 0; i < childNodes.Length; i++)
        {
            this.childNodes[i] = new Node<T>(childNodes[i]);
        }
    }

    /// <summary>
    /// Whether this is a leaf node.
    /// </summary>
    /// <returns>True if this is a leaf node</returns>
    internal bool IsLeafNode()
    {
        // if we don't have nodes, we're a leaf node 
        if (childNodes == null || childNodes.Length == 0)
            return true;
        else
            return false;
    }
}
