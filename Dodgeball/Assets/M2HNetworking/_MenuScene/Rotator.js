#pragma strict

private var myTrans : Transform;
public var rotateSpeed : int = 20;

function Awake(){
	myTrans=transform;
}

function Update () {
	myTrans.Rotate(Vector3(Time.deltaTime*rotateSpeed,0,0));
}