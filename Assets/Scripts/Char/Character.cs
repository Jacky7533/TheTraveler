﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GSP.Char
{
	public class Character : MonoBehaviour
	{
		// Declare our private variables. The default scope is private.
		Resource m_resource;			// This is the resource script object.
		int m_maxWeight;				// This is the maximum weight the character can hold.
		int m_currency; 				// This is the amount of currency the character is holding.
		GameObject m_owner;				// This is the owner of the character if any.
		List<GameObject> m_allyList;	// This is the ally list for the character.

		// Gets the current value of resources the character is holding.
		public int ResourceValue
		{
			get { return m_resource.Amount; }
		} // end ResourceValue property

		// Gets the current value of resources the character is holding.
		public int ResourceWeight
		{
			get { return m_resource.Amount; }
		} // end ResourceWeight property

		// Gets or Sets the maximum weight the character can hold.
		public int MaxWeight
		{
			get { return m_maxWeight; }
			set
			{
				// Check if the value is zero or less.
				if (value <= 0)
				{
					// Clamp the max weight to zero.
					m_maxWeight = 0;
				} // end if statement
				else
				{
					// Otherwise just set it to the value.
					m_maxWeight = value;
				} // end else statement
			}
		} // end MaxWeight property

		// Gets the currency a character is holding.
		public int Currency
		{
			get { return m_currency; }
		} // end Currency property

		// Gets and Sets the owner of the character if any.
		public GameObject Owner
		{
			get { return m_owner; }
			set { m_owner = value; }
		} // end Owner property

		// Gets the number of allies the character has.
		public int NumAllies
		{
			get { return m_allyList.Count; }
		} // end NumAllies function

		// Use this for initialization
		void Start()
		{
			// Initialise the variables.
			m_resource = GetComponent<Resource>();
			m_maxWeight = 300;
			m_currency = 0;
			m_owner = null;
			m_allyList = new List<GameObject>();
		} // end Start function
		
		// Update is called once per frame
		void Update()
		{
			
		} // end Update function

		// Picks up a resource for the character
		public void PickupResource( int resourceValue, int resourceWeight )
		{
			// Check if picking up this resource will put the character overweight.
			if ( ( ResourceWeight ) <= MaxWeight )
			{
				// Add the resource.
				m_resource.AddResource( resourceValue, resourceWeight );
			} // end if statement

			// TODO: Maybe display the can't pickup the resource?
		} // end PickupResource function

		// Sells the resources the character is currently holding.
		public void SellResource()
		{
			// If the character has an owner (is an ally) transfer the currency to the its owner.
			if ( Owner != null )
			{
				Character ownerCharScript = Owner.GetComponent<Character>();
				ownerCharScript.AddCurrency( m_resource.Amount );
			} // end if statement
			else
			{
				// Credit the character for the resources they are holding.
				AddCurrency( m_resource.Amount );
			} // end else statement

			// Clear the resources now.
			m_resource.ClearResources();
		} // end SellResource function

		// Adds the given amount of currency to what the character is currently holding.
		public void AddCurrency( int amount )
		{
			m_currency += amount;
		} // end AddCurrency function

		// Removes the given amount of currency from what the character is currently holding.
		public void RemoveCurrency( int amount )
		{
			// Check if the operation would result in a negative number.
			if ((m_currency - amount) <= 0)
			{
				// Set the currency to zero.
				m_currency = 0;
			}
			else
			{
				// Otherwise just subtract the given amount
				m_currency -= amount;
			}
		} // end RemoveCurrency function

		// Adds an ally to the list.
		public void AddAlly( GameObject ally )
		{
			m_allyList.Add( ally );
		} // end AddAlly function

		// Removes an ally from the list while optionally destroying it after.
		public void RemoveAlly( GameObject ally, bool destroy = false )
		{
			m_allyList.Remove( ally );

			if (destroy)
			{
				// Destroy the ally game object now.
				Destroy( ally );
			} // end if statement
		} // end RemoveAlly function

		// Removes all allies with the game object this script is attached to as their owner.
		// Optionally destroying them after.
		public void RemoveAllAllies( bool destroy = false )
		{
			// Find all characters that are tagged as Ally.
			GameObject[] allies = GameObject.FindGameObjectsWithTag( "Ally" );

			// Loop through the array of allies.
			for ( int index = 0; index < allies.Length; index++ )
			{
				print("i: " + index);

				// Get the character script of the current ally.
				Character charScript = allies[index].GetComponent<Character>();

				// Check the owner of the ally to see if there is a match.
				if ( charScript.Owner == this.gameObject )
				{
					// There was a match so remove the ally at that spot.
					RemoveAlly( allies[index], destroy );
				} // end if
			} // end for loop

		} // end RemoveAllAllys function
	} // end Character class
} // end namespace
