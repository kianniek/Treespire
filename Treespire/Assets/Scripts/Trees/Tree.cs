using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree <T> 
{
    // to keep track of the whole tree,
    // only keep a reference to the root node
    // since it keeps references to child nodes
    public Node<T> rootNode;

    /// <summary>
    /// Constructor for new empty tree
    /// </summary>
    public Tree() { }

    public Tree(Node<T> rootNode)
    {
        // create cloned root 
        this.rootNode = new Node<T>(rootNode);
    }
}
