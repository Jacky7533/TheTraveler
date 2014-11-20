using UnityEngine;
using System.Collections;
using System.Collections.Generic;	//required for List<GameObjects>

namespace GSP
{
	
	public class GameplayStateMachine : MonoBehaviour 
		//====================================================================
		// +will be in charge of designing the stateMachine
		// +OnGUI() //need this function to use Unity's GUI
		//			//also runs StateMachine() every cycle
		// -StateMachine() 	//runs all the State Specific GUI tasks
		// -ShowResources()	//toggles between hiding and showing Resources in the GUI bar
		//
		// +GetState() returns int
		//		//BEGINTURN
		//		//ROLLDICE
		//		//CALCDISTANCE
		//		//DISPLAYDISTANCE
		//		//SELECTPATHTOTAKE
		//		//DOACTION
		//		//ENDTURN
		// +public void NextState() will move state machine to the next state
		//
		//====================================================================
	{
		public enum GamePlayState
			//--------------------------------------------
			// +enum will hold the values of gameplay states
			//--------------------------------------------
		{
			BEGINTURN,
			ROLLDICE,
			CALCDISTANCE,
			DISPLAYDISTANCE,
			SELECTPATHTOTAKE,
			DOACTION,
			ENDTURN
		} //end public enum GamePlayState
		

		//......Holds Current State......
		public GamePlayState m_gamePlayState;
		
		//......GUI Values......
		string m_GUIActionString;	//Changes the String in the Action button According to State player is in
		string m_MapEventString; 	//holds what type of action event will occur
		string m_MapEventResourceString; 	//if mapEvent is Item, what Resource is picked up? Null if not a resource event
		bool m_GUIRunMapEventOnce = false;
		bool m_GUIActionPressed = false;	//determines if the Action Button has been pressed.
		int m_GUIPlayerTurn = 1;	//whos turn is it
		int m_GUIGoldVal = 0;
		int m_GUIWeight = 0;		//Players actual weight
		int m_GUIMaxWeight = 100;	//Players max weight
		bool m_GUIShowResources = false;	// GUI for Resources can be hidden at push of button
		int m_GUIOre = 0;
		int m_GUIWool = 0;
		int m_GUIDiceDistVal = 0;	//HOlds the dice value, then is converted into Distance Value
		int m_GUINumOfPlayers = 2; 	// how many players playing
		
		//...Script Instances...
		private GSP.DieInput m_DieScript;	//Access the sigleton Die and its functions
		private GSP.GUIMapEvents m_GUIMapEventsScript; //Access the sigleton Die and its functions
		private GSP.GUIMovement m_GUIMovementScript;
		
		//players list
		private GameObject m_playerEntity; //player!
		private List<GameObject> m_playerList;	//create players
		private List<GSP.Char.Character> m_playerScriptList; //access Character.cs scripts from each player to get player values
		private List<GSP.Char.ResourceList> m_PlayerResourceList; //access Characters.cs resourcesList scripts

		//=================================================================================================
		//-----------------------------------Functions-----------------------------------------------------
		//=================================================================================================
		
		void Start()
			//------------------------------------------------------------------
			//  Start() runs when the object if first instantiated
			//		-Because this object will occur once through the game, 
			//			these values are the beginning of game values
			//		-Values should be updated at EndTurn State of StateMachine()
			//-------------------------------------------------------------------
		{
			//initialize empty lists
			m_playerList = new List<GameObject>();
			m_playerScriptList = new List<GSP.Char.Character>();
			m_PlayerResourceList = new List<GSP.Char.ResourceList>();
			
			//TODO: get num of players from BrentsStateMachine
			
			//Add Players Instances
			AddPlayers (m_GUINumOfPlayers);

			//Beginning State
			m_gamePlayState = GamePlayState.BEGINTURN;
			
			//get scripts needed
			m_DieScript = GameObject.FindGameObjectWithTag("DieTag").GetComponent<DieInput>();
			m_GUIMapEventsScript = GameObject.FindGameObjectWithTag("GUIMapEventSpriteTag").GetComponent<GUIMapEvents>();
			m_GUIMovementScript = GameObject.FindGameObjectWithTag("GUIMovementTag").GetComponent<GSP.GUIMovement>();

			//text for the action button
			m_GUIActionString = "Action\nButton";

			//no map event at start;
			m_MapEventString = "NOTHING";
			m_MapEventResourceString = null;
		}
		
		private void AddPlayers( int p_numOfPlayers )
		{
			Vector3 startingPos = new Vector3 (32, 32, -1.6f); //first tile
			float tmpTransY = 0.0f; 
			
			for (int count = 0; count < p_numOfPlayers; count++) 
			{
				startingPos.y = 32 +((count+1) *64); 
				//create players
				m_playerEntity = Instantiate( PrefabReference.prefabCharacter, startingPos, Quaternion.identity ) as GameObject;
				m_playerEntity.transform.localScale = new Vector3 (100, 100, 1);
				
				m_playerList.Add( m_playerEntity ); //add PlayerEntity to list
				
				m_playerScriptList.Add( m_playerEntity.GetComponent<GSP.Char.Character>() );
				m_PlayerResourceList.Add( m_playerEntity.GetComponent<GSP.Char.ResourceList>() );
				
			} //end for loop
			
		} // end private void AddPlayers( int p_numOfPlayers )

		private void GetPlayerValues()
			//-----------------------------------------------------------------------------
			//	At the BEGINTURN state, values are grabbed from each player and stored into
			//		respective m_GUI variable.
			//
			//-----------------------------------------------------------------------------
		{
			m_GUIGoldVal = m_playerScriptList [m_GUIPlayerTurn].Currency;
			m_GUIMaxWeight = m_playerScriptList [m_GUIPlayerTurn].MaxWeight;
			//TODO: does this weight need to be added with armor and weapon weight??? Ask
			// 		Brent how to get Totale Weapon and armor weight Values for varible below.
			m_GUIWeight = m_playerScriptList [m_GUIPlayerTurn].ResourceWeight;
			
			//get the resources for current player
			m_GUIOre = m_PlayerResourceList[m_GUIPlayerTurn].GetResourceByType("ORE").SizeValue;
			m_GUIWool = m_PlayerResourceList[m_GUIPlayerTurn].GetResourceByType("WOOL").SizeValue;
			
		}	//end private void GetPlayerValues()

		
		void OnGUI()
			//-------------------------------------------------------------------
			//	OnGUI() is needed in order to run GUI. functions. Runs once per cycle
			//	-will run StateMachine() every cycle to make sure on correct State
			//	-Uses some config functions for Cleaning up GUI look and special occasions
			//--------------------------------------------------------------------
		{
			StateMachine();	//update any values that affect GUI before creating GUI
			
			//Buttons will be red
			GUI.backgroundColor = Color.red;
			
			//This is the tool bar container
			GUI.Box( new Rect(0,0,Screen.width,64)," ");
			
			//Scalable values for GUI miniContainers
			int gap = 2;
			int width = (Screen.width/4)-2;
			int height = 28;
			
			//..................................
			//   ...PLAYER AND GOLD COLUMN...
			//..................................
			int col = 0;
			GUI.Box (new Rect ((col*width)+(col+1)*gap, 2,width, height), "Player: "+m_GUIPlayerTurn.ToString());		//Whose Turn
			GUI.Box (new Rect ((col*width)+(col+1)*gap,32, width, height), "Gold: $"+m_GUIGoldVal.ToString());			//How Much Gold
			
			//..................................
			// ...WEIGHT AND RESOURCE COLUMN...
			//..................................
			col = col+1;
			//TODO: Instead of displaying "Weight: " Maybe better info will be given
			//if GUI dispay "Weight/MaxWeight\n" + 
			GUI.Box(new Rect ((col*width)+(col+1)*gap,2,width, height), "Weight:"+m_GUIWeight.ToString() +"/"+ m_GUIMaxWeight.ToString() );	//weight container
			ResourceButtonConfig (gap, col, width, height);
			//Show/hides Resources
			ShowResources();
			
			//..................................
			//      ...DICE ROLL COLUMN...
			//..................................
			col = col+1;
			DiceBoxConfig(gap, col, width, height);
			
			//..................................
			//      ...ACTION COLUMN...
			//..................................
			col = col + 1;
			ActionButtonConfig (gap, col, width, height);
			
		}	//end OnGUI()
		
		private void ShowResources()
			//----------------------------------------------------
			//	Hides and shows the Resources
			//
			//----------------------------------------------------
		{
			if (m_GUIShowResources) 
			{
				//Resrouces GUI Container attributes
				const int NUMOFRSRCS = 2; //change this to meet our needs
				int gap = 2;
				int width = (Screen.width/NUMOFRSRCS)-2;
				int height = 28;
				
				//...............
				//   ...Ore...
				//..............
				int col = 0;
				GUI.Box(new Rect ((col*width)+(col+1)*gap,64,width, height), "Ore: "+m_GUIOre.ToString());
				
				//...............
				//  ...Wool...
				//...............
				col = col +1;
				GUI.Box(new Rect ((col*width)+(col+1)*gap,64,width, height), "Wool: "+m_GUIWool.ToString());
				//GUI.Box(new Rect ((col*width)+(col+1)*gap,2,width, height), "Weight: "+m_GUIGoldVal.ToString());
				//GUI.Box(new Rect ((col*width)+(col+1)*gap,2,width, height), "Weight: "+m_GUIGoldVal.ToString());
			}	//end if(m_GUIShowResources)
			
		} //end private void ShowResources()
		
		
		private void ResourceButtonConfig(int p_gap, int p_col, int p_width, int p_height)
			//--------------------------------------------------------------------------------
			//	-Configures Resource Button to display correctly on screen
			//	-Toggles betweend ShowResources or Hide Resources boolean
			//
			//--------------------------------------------------------------------------------
		{
			if (GUI.Button (new Rect ((p_col * p_width) + (p_col + 1) * p_gap, 32, p_width, p_height),"Resources")) 		//resrouces toggle Show/Hide Button
			{
				
				if ( !(m_GUIShowResources) ) 
				{	m_GUIShowResources = true; }	//Show Resources Containers
				else 
				{ m_GUIShowResources = false; }		//Hide Resources' Containers
				
			}	//end if(GUI.Button(Resources))
			
		}	//end private void ResourceButtonConfig( int p_gap, int p_col, int p_width, int p_height )
		
		
		private void DiceBoxConfig( int p_gap, int p_col, int p_width, int p_height )
			//--------------------------------------------------------------------------
			//	-Configures DiceBox container in GUI
			//	-Special Occassion: When the Dice has not been rolled, is should display
			//		"DICE ROLL [Press Action Button]"
			//
			//--------------------------------------------------------------------------
		{
			//if in first state, tell the user to push the Action Button to begin rolling the Die
			if (m_gamePlayState == GamePlayState.ROLLDICE) 
			{
				GUI.Box (new Rect ((p_col * p_width) + (p_col + 1) * p_gap, 2, 
				                   p_width, 2 * p_height), "DICE ROLL\n[Press Action\nButton]");
			} 
			else
				//else display the Rolled Value
			{
				GUI.Box (new Rect ((p_col * p_width) + (p_col + 1) * p_gap, 2,
				                   p_width, 2 * p_height), "Travel Dist.\n\n"+m_GUIDiceDistVal);
			}
			
		}	//end private void DiceBoxConfig( int gap, in col, int width, int height )
		
		
		private void ActionButtonConfig( int gap, int col, int width, int height )
			//-----------------------------------------------------------------------------
			//	Action button has a few different settings to consider,
			//		-Makes sure Action button gets pressed only once per state
			//		 
			//------------------------------------------------------------------------------
		{
			//Makes sure Action Button gets pressed only once per State
			if (!(m_GUIActionPressed)) {
				if (GUI.Button (new Rect ((col * width) + (col + 1) * gap, 2, width, 2 * height), m_GUIActionString)) 
				{
					m_GUIActionPressed = true;
				}
			} 
			else 
			{
				GUI.Box (new Rect ((col * width) + (col + 1) * gap, 2, width, 2 * height), m_GUIActionString);
			}
	
		} //end private void ActionButtonConfig(int gap, int col, int width, int height)

		private void MovementButtonConfig()
		{

		} //end private void MovementButtonConfig()

		
		private void StateMachine()
			//---------------------------------------------------------------------
			// +StateMachine in charge of displaying GUI that describes
			//	 what stage the gamePlay is in
			// +Uses a switch() statement for handling states
			//
			//---------------------------------------------------------------------
		{
			var state = GameObject.FindGameObjectWithTag("GUITextTag").GetComponent<GUIText>();
			switch (m_gamePlayState) 
			{

			case GamePlayState.BEGINTURN:
				//Get All the Players values
				//				GetPlayerValues();
				m_gamePlayState = GamePlayState.ROLLDICE;
				break;

			case GamePlayState.ROLLDICE:
				
				state.text = "Player " +m_GUIPlayerTurn.ToString() +" roll dice";
				//create a roll dice button
				m_GUIActionString = "Action\nRoll Dice";
				//if button is clicked, destroy button
				if( m_GUIActionPressed )
				{
					m_GUIDiceDistVal = m_DieScript.Dice.Roll(1,6);
					//nextState()
					m_gamePlayState = GamePlayState.CALCDISTANCE;
				}					
				break;

			case GamePlayState.CALCDISTANCE:
				state.text = "Calculate Distance";

				//get dice value /calculate m_allowedTravelDistance
				m_GUIDiceDistVal = (m_GUIMaxWeight-m_GUIWeight)/m_GUIMaxWeight;

				//nextState()
				m_GUIActionPressed = false;
				m_gamePlayState = GamePlayState.DISPLAYDISTANCE;
				break;

			case GamePlayState.DISPLAYDISTANCE:
				state.text = "DisplayDistance";

				//TODO: Add function to display tiles

				//nextState()
				m_gamePlayState = GamePlayState.SELECTPATHTOTAKE;
				break;

			case GamePlayState.SELECTPATHTOTAKE:
				state.text = "Select Path To Take\nPress 1 to EndTurn,\nPress 2 to Do Action";
				m_GUIActionString = "End Turn";

				//display arrows
				m_GUIMovementScript.InitThis( m_GUIDiceDistVal );

				//if ( Input.GetKeyDown( KeyCode.Alpha1 ) )
				if ( m_GUIActionPressed )
				{
					m_gamePlayState = GamePlayState.ENDTURN;
					m_GUIActionPressed = false;
				}

				//if Mapevent occurs
				if ( Input.GetKeyDown( KeyCode.Alpha2 ) )
				{
					m_gamePlayState = GamePlayState.DOACTION;
					m_GUIMapEventsScript.InitThis( m_playerList[m_GUIPlayerTurn], "ENEMY", null );
					//TODO: after testing, delete command above and use this one below
					//m_GUIMapEventsScript.InitThis( m_playerList[m_GUIPlayerTurn], m_MapEventString );
				}
				break;

			case GamePlayState.DOACTION:
				state.text = "Do Action/MapEvent";

				//map events
				if( !(m_GUIMapEventsScript.isActionRunning()) )
				{
					//TODO: After Testing, uncomment the following
					//m_MapEventString = "NOTHING"
					//m_MapEventResourceString = NULL;

					//send back to previous state
					m_gamePlayState = GamePlayState.SELECTPATHTOTAKE;
				}
				//NextState()
				break;

			case GamePlayState.ENDTURN:
				state.text = "End Turn";
				
				//next players turn
				m_GUIPlayerTurn = m_GUIPlayerTurn +1;
				//if exceeds the number of player playing, start back at player 1
				if( m_GUIPlayerTurn > m_GUINumOfPlayers )
				{
					m_GUIPlayerTurn = 1;
				}
				
				m_gamePlayState = GamePlayState.BEGINTURN;
				break;
				
			default:
				//stage transition
				break;
				
			} //end switch (m_gamePlayState)
			
		} //end private void StateMachine()

		
		public int GetState()
			//---------------------------------------------------------
			//	-If another class wants to get the Current State, 
			//		this function will return int form of Enum States
			//
			//----------------------------------------------------------
		{
			int state;
			state = (int)m_gamePlayState;
			return state;
		}
		
		public void NextState()
			//---------------------------------------------------------
			// changes to the next state
			//		-MIGHT NOT BE NEEDED IF GUI CONTROLS
			//			WHICH STATES COME NEXT
			//----------------------------------------------------------
		{
			m_gamePlayState = m_gamePlayState +1;
		} //end public void NextStage()

		public void DoAction( string p_MapEventType, string p_TypeOfResource )
			//--------------------------------------------------------
			//	can be called from other objects to start an action
			//
			//--------------------------------------------------------
		{
			//set state machine to DOACTION
			m_gamePlayState = GamePlayState.DOACTION;

			//set map event typs and call the init
			m_MapEventString = p_MapEventType;
			m_MapEventResourceString = p_TypeOfResource;
		} // void 

		public void EndTurn()
			//---------------------------------------------------------
			// automatically changes to endTurn state
			// and resets values
			//---------------------------------------------------------
		{
			m_gamePlayState = GamePlayState.ENDTURN;
			
		} //end public void ENDTURN()
		
		private void ResetValues()
			//---------------------------------------------------------
			// Resets Values at the end of a Turn right before a New 
			// 	New Players Turn
			//
			//---------------------------------------------------------
		{
			m_GUIActionString = "Action\nButton";
			m_GUIActionPressed = false;
			m_GUIGoldVal = 0;
			m_GUIWeight = 0;
			m_GUIMaxWeight = 100;
			m_GUIShowResources = false;
			m_GUIOre = 0;
			m_GUIWool = 0;
			m_GUIDiceDistVal = 0;
			m_GUIRunMapEventOnce = false;
			
		}	//end private void ResetValues();

		public GameObject GetCurrentPlayer()
		{
			return	m_playerList[m_GUIPlayerTurn];
		} //end public GameObject Get

		public void OnDrawGizmos()
			//---------------------------------------------------------
			//to draw on the scene.
			//	-ORIGINALLY GOING TO BE USED FOR A DIFFERENT
			//		TYPE OF UNITY GUI, BUT NO LONGER NEEDED
			//---------------------------------------------------------
		{
			
		} //end public void OnDrawGizmos()
		
	}	//end public class GameplayStateMachine : MonoBehaviour
	
}	//end namespace GSP
