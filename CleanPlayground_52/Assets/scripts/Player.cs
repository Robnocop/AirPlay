using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
    public Transform collisionMeshPrefab;
    //[HideInInspector]public GameObject[] lines;
    [HideInInspector]public GameObject collisionMeshObject;
    public float positionx;
	public float positiony;
	public int id = -1;
	public int trackerid =-1; 
	public int gameid = -1;
    public Color lineColor = Color.blue;
    public int numLines=100;
    public GameObject[] lines;

    //this is automatically set at the start of the game and upon a change of number of alive players
    Vector3 targetPos = new Vector3(0,0,0);
	Vector3 startPosition = new Vector3(0,0,0);
	private float lastUpdateTime;
	public float lastDurationUpdateTime;
	public float dieThresholdTime =3.0f;
	public bool death = false;
	bool previousDeath = false;

	public bool woozIsOn =false;
	public bool singleWoozPlayer;
	public float singleWoozDeltatimeMultiplier = 100.0f; //changes the speed with which the wooz player is moved

    private float lastDrawTime;
    Vector3 lastDrawPosition;
    //TODO re add moveVectorK it was used for the wooz
    Vector3 moveVectorK;

    //EXAMPLE of an old logger, indicating if the update took to long, saving those moments in which it did
    //private bool updateDeath = false;
    //bool previousUpdateDeath = false;
    //public float updateDieThresholdTime = 0.08f;
    
	//keep count of "speed" 
	public Vector2[] positionsForSpeed;
	public int sizeOfSpeedVector = 15;
	//!!!! assign this in the scene per player it should be the actual arrow group in the scene not a prefab, thus it is assigned in the scene not in the prefab!
	private GameObject mainCameraObject;

	private Logger loggerScript;
	private BackGroundChanger backgroundChangerScript;
	private GameObject backgroundImageObject;

    private Vector3[] trail;
    private int trailCounter;
    //public buildMesh missile;
    //you might want to switch materials based on somec conditions
    //Material taggerMat, runnerMat;

    // Use this for initialization
    void Start () {



        //buildMesh missileCopy = Instantiate<buildMesh>(missile);

        trail = new Vector3[numLines];
        trailCounter = 0;
        lines = new GameObject[numLines];
		//average speed over 5 frames
		initiatePositionVector();

    //be able to switch the background if something happends
        backgroundImageObject = GameObject.FindGameObjectWithTag("BackgroundImage");
		mainCameraObject = GameObject.FindGameObjectWithTag("MainCamera");
		backgroundChangerScript = backgroundImageObject.GetComponent("BackGroundChanger") as BackGroundChanger;

		loggerScript = mainCameraObject.GetComponent("Logger") as Logger;

		startPosition = this.gameObject.transform.position;
        lastDrawTime = Time.realtimeSinceStartup;
        lastDrawPosition = startPosition;

        targetPos = startPosition;

		//keep track of last update time, be able to show and hide players based on this
		lastUpdateTime = Time.realtimeSinceStartup;
		lastDurationUpdateTime = 1.0f/25.0f; //approximate framerate


        //you can change the material of the player in such a way:
        //this.renderer.material = taggerMat;
        //else
        //	this.renderer.material = runnerMat;
        trail[0] = this.transform.localPosition;

    }

	// Update is called once per frame
	void Update () {

        //WoozPlayer are wooz of oz players, we switch between normal server controlled players and wooz with pressing W in the game
        if (singleWoozPlayer)
		{
			singleWoozKeyMovePlayerInputHandler();
			lastUpdateTime = Time.realtimeSinceStartup;
		}

		if (woozIsOn)
		{
			lastUpdateTime = Time.realtimeSinceStartup;
		}

		//if a player hasnt been update in the previous seconds set it to a dead player and for instance later on hide it etc.
		if ((Time.realtimeSinceStartup-lastUpdateTime)>dieThresholdTime)
		{
			death = true;
			//also reset the time he/she has been a tagger we only use this in expo mode after all and we do not use adaptive circles here
		}
		else
		{
			death = false;
		}

		if (death!= previousDeath)
		{
			if (death)
			{
				//hide the player and turn of its collider child.
				this.transform.GetComponent<Renderer>().enabled = false;
				Transform colliderChild = this.transform.FindChild("Collider");
				colliderChild.GetComponent<Collider>().enabled = false;

				//move towards start position 
				this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, startPosition, Time.deltaTime);

				//you might want to reset the material
				//this.renderer.material = runnerMat;

			}
			else
			{
				//make player visible again
				this.transform.GetComponent<Renderer>().enabled = true;
				Transform colliderChild = this.transform.FindChild("Collider");
				colliderChild.GetComponent<Collider>().enabled = true;
			}		

			previousDeath = death;
		}
		//Collisions etc are calculated after fixedupdate, therefore moving rigidbodies should be dealt with in the fixedupdate
		//thus we move this to fixed update http://unity3d.com/learn/tutorials/modules/beginner/scripting/update-and-fixedupdate
		//similar to death this is called only once and never changed
		else if (death)
		{
			//death = true;
			//previousDeath = true;
			//this.transform.renderer.enabled = false;
			//Transform colliderChild = this.transform.FindChild("Collider");
			//colliderChild.collider.enabled = false;
			
			//move towards start position 
			this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, startPosition, Time.deltaTime);
		}

		//EXAMPLE of another logger
		//this code was able to indicate moments in which the player hasnt been updated for a short amount of time, some frames have been dropped kind of thing:
		//if ((Time.realtimeSinceStartup-lastUpdateTime)>updateDieThresholdTime)
		//{
		//	updateDeath = true;
		//}
		//else
		//{
		//	updateDeath = false;
		//}

		//if (updateDeath != previousUpdateDeath)
		//{
		//	loggerScript.LogLineMissingUpdate(id, updateDeath);
		//	previousUpdateDeath = updateDeath;
		//}


	}

	void FixedUpdate()
	{
// print(lastDrawPosition + " " + this.transform.localPosition);

        if(Mathf.Abs(trail[trailCounter % numLines].x-this.transform.localPosition.x) + Mathf.Abs(trail[trailCounter % numLines].z-this.transform.localPosition.z) >0.25)
        {
            trailCounter = trailCounter + 1;
            trail[trailCounter % numLines] = this.transform.localPosition;
           
   
         
            GameObject.Destroy(lines[trailCounter % numLines]);
            print("# of lines: "+lines.Length + " -- counter: " + (trailCounter % numLines));
            lines[trailCounter % numLines] = new GameObject();
            lines[trailCounter % numLines].name = "Line" + trailCounter;
            lines[trailCounter % numLines].transform.position = trail[trailCounter % numLines];
            lines[trailCounter % numLines].AddComponent<LineRenderer>();
            LineRenderer lr = lines[trailCounter % numLines].GetComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
            lr.SetColors(lineColor, lineColor);
            lr.SetWidth(0.1f, 0.1f);
            lr.SetPosition(0, trail[trailCounter%numLines]);
            lr.SetPosition(1, trail[(trailCounter-1)%numLines]);
            print(trail[0]);
            for(int i = 1; i < numLines;i++)
            {
                if(Mathf.Abs(trail[trailCounter % numLines].x - trail[(trailCounter+i)%numLines].x) + Mathf.Abs(trail[trailCounter % numLines].z - trail[(trailCounter + i) % numLines].z) < 0.25){
                    print("COLLISION!!@@#@#");
                    //ransform collisionMesh;
                    //collisionMesh = Instantiate(collisionMeshPrefab) as Transform;

                    collisionMeshObject = Instantiate(collisionMeshPrefab).transform.gameObject;
                    collisionMeshObject.name = "Collision!";
                    Vector3[] collisionVertices = new Vector3[Mathf.Abs(trailCounter%numLines - i)+1];

                    Vector3 average = new Vector3(0, 0, 0);
                    int counter = 0;
                    if(Vector3.SqrMagnitude(trail[trailCounter%numLines+1]-Vector3.zero)>0.001) // Go from traiLCounter to i positive steps
                    {
                        print("go from trailcounter to i");
                        print("trail trailcounter-1" + trail[trailCounter%numLines - 1] + " trail trailcounter+1" + trail[trailCounter%numLines + 1]);
                        if (i < trailCounter % numLines) // use i+numLines
                        {
                            for (int p = trailCounter%numLines; p < i+numLines; p++)
                            {
                                collisionVertices[counter] = trail[p%numLines];
                                average = average + trail[p%numLines];
                                counter = counter + 1;
                                print("p " + p + " trail" + trail[p % numLines]);
                            }
                        }
                        else // use i
                        {
                            for (int p = trailCounter % numLines; p < i; p++)
                            {
                                collisionVertices[counter] = trail[p % numLines];
                                average = average + trail[p % numLines];
                                counter = counter + 1;
                                print("p " + p + " trail" + trail[p % numLines]);
                            }
                        }
                    }
                    else // Go negative from trailCounter
                    {
                        if (i < trailCounter % numLines) // use trailcounter
                        {
                            for (int p = trailCounter; p >i; p--)
                            {
                                collisionVertices[counter] = trail[p % numLines];
                                average = average + trail[p % numLines];
                                counter = counter + 1;
                                print("p " + p + " trail" + trail[p % numLines]);
                            }
                        }
                        else // use trailcounter+numlines
                        {
                            for (int p = trailCounter+numLines; p >i; p--)
                            {
                                collisionVertices[counter] = trail[p % numLines];
                                average = average + trail[p % numLines];
                                counter = counter + 1;
                                print("p " + p + " trail" + trail[p % numLines]);
                            }
                        }
                    }
                    
                    average = average / (numLines-1);
                    print("i: " + i + " trailCounter % numLines " + trailCounter % numLines);
                    print("Average: " + average + " last vertice: " + collisionVertices[Mathf.Abs(trailCounter % numLines - i)] + " first vertice " + collisionVertices[0] + " 2nd vertice "+ collisionVertices[1]);
                    collisionVertices[counter] = average;
                        //{
                        //new Vector3(1,0,-1), // top left
                        //new Vector3(2,0,-1), // top right
                        //new Vector3(1,0,-3), // bottom left
                        //new Vector3(2,0,-3), // bottom right
                        //new Vector3(1,0,-1),
                        //new Vector3(2,0,-1),
                        //new Vector3(2,0,-3),
                        //new Vector3(1,0,-3),
                        //new Vector3(0.5f,0,-2),
                        // AVERAGE
                        //new Vector3(1.3f,0,-2)

                        //};
                    collisionMeshObject.GetComponent<buildMesh>().setVertices(collisionVertices);
                    trail = new Vector3[numLines];
                    trailCounter = 0;
                    trail[0] = this.transform.localPosition;
                    for (int j = 1; j < numLines; j++)
                    {
                        GameObject.Destroy(lines[j]);
                    }
                    lines = new GameObject[numLines];
                }
            }

        }
        

		if(!woozIsOn && !singleWoozPlayer)
		{
			//the updaterate from the moveTo is taken from the kinectrig and is approximmetly 30fps the lastUpdateTime and the lastDuration are also set there
			//the update of update is supposed to be quicker
			float fracComplete = (Time.realtimeSinceStartup - lastUpdateTime) / lastDurationUpdateTime;

            //Collisions etc are calculated after fixedupdate, therefor rigidbodies should be done in fixedupdate
            //thus we move this to fixed update http://unity3d.com/learn/tutorials/modules/beginner/scripting/update-and-fixedupdate
            //changed back to lerp to provide better collision detection and smoother movement etc.....
            //TODO make an extrapolation

            
			this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, targetPos, fracComplete);//slowDownMovementMultiplier*fracComplete);
			//this.transform.localPosition = targetPos;
		}
		else {

            this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, this.transform.localPosition+moveVectorK, singleWoozDeltatimeMultiplier*Time.deltaTime);
			//TODO check if this works
			//the player should stop moving when the button is not pressed
			moveVectorK = Vector3.zero;
		}

	}

	private void initiatePositionVector() {
		
		positionsForSpeed = new Vector2[sizeOfSpeedVector];
		positionsForSpeed[0].x = gameObject.transform.localPosition.x;
		//we will never update y
		//positionsForSpeed[0].y = gameObject.transform.localPosition.y;
		positionsForSpeed[0].y = gameObject.transform.localPosition.z;
		for (int i=0;i<positionsForSpeed.Length;i++)
		{
			positionsForSpeed[i] = positionsForSpeed[0];
		}
	}

	//THERE IS SOMETHING STRANGE HAPPENNING with these values
	public Vector2 get2DSpeedVector() {
		//if I divide by correct number of frames here it goes wrong if I do it later on it is done correct..
		float xdiff = (positionsForSpeed[sizeOfSpeedVector-1].x-positionsForSpeed[0].x);
		float ydiff = (positionsForSpeed[sizeOfSpeedVector-1].y-positionsForSpeed[0].y);
		Vector2 returnVector = new Vector2(xdiff,ydiff);
		return returnVector;
	}

	private void updatePositionVector(Vector3 latestPosition) {
//		if (getIsTagger())
//		{	
//			print("last pos" + latestPosition + " get2DSpeedVector" + get2DSpeedVector() + "pos0" + positionsForSpeed[0] + "last" + positionsForSpeed[sizeOfSpeedVector-1] );
//		}

		for (int i=0;i<positionsForSpeed.Length-1;i++)
	    {
			positionsForSpeed[i] = positionsForSpeed[i+1];
//			positionsForSpeed[0] = positionsForSpeed[1];
//			positionsForSpeed[1] = positionsForSpeed[2];
//			positionsForSpeed[2] = positionsForSpeed[3];
//			positionsForSpeed[3] = positionsForSpeed[4];
//			positionsForSpeed[4].x = latestPosition.x;
//			positionsForSpeed[4].y = latestPosition.z;
		}
		positionsForSpeed[positionsForSpeed.Length-1].x = latestPosition.x;
		positionsForSpeed[positionsForSpeed.Length-1].y = latestPosition.z;

	}

	public void updateLastDetectionTime()
	{
		float actualDuration = Time.realtimeSinceStartup - lastUpdateTime;
		if (actualDuration>1.0f)
		{
			lastDurationUpdateTime = 1.0f/30.0f;
		}
		else
		{
			//TODO perhaps use an average instead of the last value
			lastDurationUpdateTime = Time.realtimeSinceStartup - lastUpdateTime;
		}
		lastUpdateTime = Time.realtimeSinceStartup;
	}

	//only accesed via the kinectrigclient
	public void moveTo(float x, float y, float z)
	{
		updateLastDetectionTime();
		//lastUpdateTime = Time.realtimeSinceStartup;
		if(!singleWoozPlayer)
		{
			//this.transform.position = new Vector3 (x, z, y);
			targetPos.x = x;
			targetPos.y = z;
			targetPos.z = y;
		}

		//the actual moving happends inside the fixed update in order to keep a kind of steady movement, less dependent on the tracker and more reliable physics calculations
	}

	//TODO ideally for collision detection this is done in FixedUpdate
	private void singleWoozKeyMovePlayerInputHandler()
	{
	
		moveVectorK = Vector3.zero;
		if (Input.GetKey(KeyCode.UpArrow))
		{
			moveVectorK.z = 0.1f;
		}
		
		if (Input.GetKey(KeyCode.DownArrow))
		{
			moveVectorK.z = -0.1f;
		}

		if (Input.GetKey(KeyCode.RightArrow))
		{
			moveVectorK.x = 0.1f;
		}
		
		if (Input.GetKey(KeyCode.LeftArrow))
		{
			moveVectorK.x = -0.1f;
		}	
		//this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, this.transform.localPosition+moveVectorK, singleWoozDeltatimeMultiplier*Time.deltaTime);
	}
}
