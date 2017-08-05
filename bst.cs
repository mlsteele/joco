using System;
using System.Collections.Generic;

/** Red-black tree (self-balancing binary search tree, all operations log(n) **/
class Tree<T> where T : IComparable<T> {
    public T node;
    public bool isBlack;
    private bool isRoot; // null parent -> isRoot

    public Tree<T> parent;
    //private Tree<T> grandparent;
    //private Tree<T> uncle;

    public Tree<T> left;
    public Tree<T> right;

    public constructor(Tree<T> parent = null) {
    	// Null leaf node
    	this.isBlack = true;
    	this.parent = parent;
    	this.isRoot = parent == null;
    	/*
    	if (parent != null) { // Set grandparent
    	    this.grandparent = parent.parent;
	}
	if (this.grandparent != null) { // Set uncle
	    if (this.parent == this.grandparent.left) {
	    	this.uncle = this.grandparent.right;
	    } else {
	    	this.uncle = this.grandparent.left;
	    }
	}*/
    }

    private Tree<T> grandparent() {
    	if (this.parent) return this.parent.parent;
    	return null;
    }

    private Tree<T> uncle() {
    	var gp = this.grandparent();
    	if (gp) {
    	    if (this.parent == gp.left) {
    	    	return gp.right;
	    } else {
	    	return gp.left;
	    }
	}
	return null;
    }

    public void insert(T node) {
	if (this.node == null) {
	    // insert here at this node
	    this.node = node;
	    this.left = new Tree<T>(this);
	    this.right = new Tree<T>(this);
	    this.handle_insert();
	} else if (this.node.CompareTo(node) < 0) {
	    // node belongs in the left
	    this.left.insert(node);
	} else {
	    // node belongs in the right
	    this.right.insert(node);
	}
    }

    public void delete() {
	// TODO(jessk)
    }

    private void handle_insert() {
    	// Case 1: Root
	if (this.isRoot) {
	    this.isBlack = true;
	    return;
	}
	// Case 2: Parent is black
	if (this.parent.isBlack) {
	    return;
	}
	// Case 3: Parent & Uncle are red
	if (this.uncle().isBlack) {
	    this.parent.isBlack = true;
	    this.uncle().isBlack = true;
	    this.grandparent().isBlack = false;
	    this.grandparent().handle_insert(); // Recurse
	    return;
	}
	// Case 4: Parent is red, Uncle is black; in rotation position
	if (this.parent.right == this && this.parent == this.grandparent().left) {
	    this.parent.rotate_left();
	    this.left.handle_insert_5();
	    return;
	} else if (this.parent.left == this && this.parent == this.grandparent().right) {
	    this.parent.rotate_right();
	    this.right.handle_insert_5();
	    return;
	}
	this.handle_insert_5();
    }

    // Case 5: Parent is red, Uncle is black; in the switch-and-rotate position
    private void handle_insert_5() {
	var g = this.grandparent();
	g.isBlack = false;
	this.parent.isBlack = true;
	if (this.parent.left == this) {
	    g.rotate_right();
	} else {
	    g.rotate_left();
	}
    }

    private void rotate_left() {
    	var takes_my_place = this.right;
    	var switches_parent = takes_my_place.left;
    	var grandparent = this.parent;

	grandparent.left = takes_my_place;
	takes_my_place.parent = grandparent;

	this.right = switches_parent;
	switches_parent.parent = this;

	takes_my_place.left = this;
	this.parent = takes_my_place;
    }

    private void rotate_right() {
	var takes_my_place = this.left;
	var switches_parenet = takes_my_place.right;
	var grandparent = this.parent;

	grandparent.right = takes_my_place;
	takes_my_place.parent = grandparent;

	this.left = switches_parent;
	switches_parent.parent = this;

	takes_my_place.right = this;
	this.parent = takes_my_place;
    }

}
