using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class SickeningMazeScript : MonoBehaviour
{
    public KMAudio Audio;
    public KMBombInfo Bomb;
    public KMBombModule Module;

    public AudioClip[] SFX;
	public AudioSource Digger;

	public GameObject Movement,Selection,Button;
	public MeshRenderer Border;
	public KMSelectable[] Arrows, Toggles;
	public TextMesh Seedling, Layout, Mode, KeyView, Traveller, Continuation;
	public TextMesh[] ToggleText;
    public KMSelectable SendIt, Checker;
	
	int[] Coordinates = new int[7];
	int[] KeyPlacementDecimal = new int[3];
	List<int> KeyPlacementGathered = new List<int>();

	string[,,,] MazeGenerated = new string[8,8,8,8];
	string[,,,] ConnectionsGenerated = new string[8,8,8,8];
	
	int MazeRow = 3, MazeColumn = 3, MazeFloor = 3, MazeTimeline = 3; 
	int ForcedSteps = 50000, Alternator =  0;
	int[] CurrentToggles = {0,0};

    bool Interactable = true, Determinable = false, Travelling = false, Striking = false;

    // Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool ModuleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
		
		for (int a = 0; a < Arrows.Count(); a++)
        {
			int Movement = a;
            Arrows[Movement].OnInteract += delegate
            {
                Moving(Movement, MazeRow, MazeColumn, MazeFloor, MazeTimeline);
                return false;
            };
		}
		
		for (int b = 0; b < Toggles.Count(); b++)
        {
			int Toggling = b;
            Toggles[Toggling].OnInteract += delegate
            {
                AlternateView(Toggling);
                return false;
            };
		}
		Checker.OnInteract += delegate () { Check(); return false; };
        SendIt.OnInteract += delegate () { Sender(); return false; };
    }

    void Start()
    {
		Selection.SetActive(false);
		KeyCoordinates();
		GenerateMazeOriginShift(MazeRow, MazeColumn, MazeFloor, MazeTimeline);
    }
	
	void GenerateMazeOriginShift(int mazerow, int mazecolumn, int mazefloor, int mazetimeline)
	{
		if (mazerow > 0 && mazecolumn > 0  && mazefloor > 0 && mazetimeline > 0 && (mazerow + mazecolumn + mazefloor + mazetimeline > 4))
		{
			MazeGenerated = new string[mazetimeline,mazefloor,mazecolumn,mazerow];
			ConnectionsGenerated = new string[mazetimeline,mazefloor,mazecolumn,mazerow];
			for (int w = 0; w < mazetimeline; w++)
			{
				for (int z = 0; z < mazefloor; z++)
				{
					for (int x = 0; x < mazecolumn; x++)
					{
						for (int y = 0; y < mazerow; y++)
						{
							//U = Up, D = Down, L = Left, R = Right, T = Top, B = Bottom, F = Future, P = Past
							MazeGenerated[w,z,x,y] = "UDLRTBFP";
							if (y < mazerow - 1)
							{
								ConnectionsGenerated[w,z,x,y] = "R";
							}
							
							else
							{
								if (x < mazecolumn - 1)
								{
									ConnectionsGenerated[w,z,x,y] = "D";
								}
								
								else
								{
									if (z < mazefloor - 1)
									{
										ConnectionsGenerated[w,z,x,y] = "B";
									}
									
									else
									{
										if (w < mazetimeline - 1)
										{
											ConnectionsGenerated[w,z,x,y] = "P";
										}
										
										else
										{
											ConnectionsGenerated[w,z,x,y] = "X";
										}
									}
								}
							}
							
						}
					}
				}
			}
			
			int CurrentRow = mazerow - 1, CurrentColumn = mazecolumn - 1, CurrentFloor = mazefloor - 1, CurrentTimeline = mazetimeline - 1;
			for (int x = 0; x < ForcedSteps; x++)
			{
				bool StepValid = false;
				do
				{
					int Movement = UnityEngine.Random.Range(0,8);
					switch(Movement)
					{
						case 0:
							if (CurrentColumn != 0)
							{
								ConnectionsGenerated[CurrentTimeline,CurrentFloor,CurrentColumn,CurrentRow] = "U";
								CurrentColumn = ((CurrentColumn - 1) + mazecolumn) % mazecolumn;
								ConnectionsGenerated[CurrentTimeline,CurrentFloor,CurrentColumn,CurrentRow] = "X";
								StepValid = true;
							}
							break;
						case 1:
							if (CurrentColumn != mazecolumn - 1)
							{
								ConnectionsGenerated[CurrentTimeline,CurrentFloor, CurrentColumn,CurrentRow] = "D";
								CurrentColumn = (CurrentColumn + 1) % mazecolumn;
								ConnectionsGenerated[CurrentTimeline,CurrentFloor, CurrentColumn,CurrentRow] = "X";
								StepValid = true;
							}
							
							break;
						case 2:
							if (CurrentRow != 0)
							{
								ConnectionsGenerated[CurrentTimeline,CurrentFloor, CurrentColumn,CurrentRow] = "L";
								CurrentRow = ((CurrentRow - 1) + mazerow) % mazerow;
								ConnectionsGenerated[CurrentTimeline,CurrentFloor, CurrentColumn,CurrentRow] = "X";
								StepValid = true;
							}
							break;
						case 3:
							if (CurrentRow != mazerow - 1)
							{
								ConnectionsGenerated[CurrentTimeline,CurrentFloor, CurrentColumn,CurrentRow] = "R";
								CurrentRow = (CurrentRow + 1) % mazerow;
								ConnectionsGenerated[CurrentTimeline,CurrentFloor, CurrentColumn,CurrentRow] = "X";
								StepValid = true;
							}
							break;
						case 4:
							if (CurrentFloor != 0)
							{
								ConnectionsGenerated[CurrentTimeline,CurrentFloor, CurrentColumn,CurrentRow] = "T";
								CurrentFloor = ((CurrentFloor - 1) + mazefloor) % mazefloor;
								ConnectionsGenerated[CurrentTimeline,CurrentFloor, CurrentColumn,CurrentRow] = "X";
								StepValid = true;
							}
							break;
						case 5:
							if (CurrentFloor != mazefloor - 1)
							{
								ConnectionsGenerated[CurrentTimeline,CurrentFloor, CurrentColumn,CurrentRow] = "B";
								CurrentFloor = (CurrentFloor + 1) % mazefloor;
								ConnectionsGenerated[CurrentTimeline,CurrentFloor, CurrentColumn,CurrentRow] = "X";
								StepValid = true;
							}
							break;
						case 6:
							if (CurrentTimeline != 0)
							{
								ConnectionsGenerated[CurrentTimeline,CurrentFloor, CurrentColumn,CurrentRow] = "F";
								CurrentTimeline = ((CurrentTimeline - 1) + mazetimeline) % mazetimeline;
								ConnectionsGenerated[CurrentTimeline,CurrentFloor, CurrentColumn,CurrentRow] = "X";
								StepValid = true;
							}
							break;
						case 7:
							if (CurrentTimeline != mazetimeline - 1)
							{
								ConnectionsGenerated[CurrentTimeline,CurrentFloor, CurrentColumn,CurrentRow] = "P";
								CurrentTimeline = (CurrentTimeline + 1) % mazetimeline;
								ConnectionsGenerated[CurrentTimeline,CurrentFloor, CurrentColumn,CurrentRow] = "X";
								StepValid = true;
							}
							break;
						default:
							break;
					}
				}
				while (!StepValid);
			}
			
			for (int w = 0; w < mazetimeline; w++)
			{
				for (int z = 0; z < mazefloor; z++)
				{
					for (int x = 0; x < mazecolumn; x++)
					{
						for (int y = 0; y < mazerow; y++)
						{
							switch(ConnectionsGenerated[w,z,x,y])
							{
								case "U":
									MazeGenerated[w,z,x,y] = MazeGenerated[w,z,x,y].Replace("U", "");
									MazeGenerated[w,z,((x-1)+mazecolumn)%mazecolumn,y] = MazeGenerated[w,z,((x-1)+mazecolumn)%mazecolumn,y].Replace("D", "");
									break;
								case "D":
									MazeGenerated[w,z,x,y] = MazeGenerated[w,z,x,y].Replace("D", "");
									MazeGenerated[w,z,(x+1)%mazecolumn,y] = MazeGenerated[w,z,(x+1)%mazecolumn,y].Replace("U", "");
									break;
								case "L":
									MazeGenerated[w,z,x,y] = MazeGenerated[w,z,x,y].Replace("L", "");
									MazeGenerated[w,z,x,((y-1)+mazerow)%mazerow] = MazeGenerated[w,z,x,((y-1)+mazerow)%mazerow].Replace("R", "");
									break;									
								case "R":
									MazeGenerated[w,z,x,y] = MazeGenerated[w,z,x,y].Replace("R", "");
									MazeGenerated[w,z,x,(y+1)%mazerow] = MazeGenerated[w,z,x,(y+1)%mazerow].Replace("L", "");
									break;
								case "T":
									MazeGenerated[w,z,x,y] = MazeGenerated[w,z,x,y].Replace("T", "");
									MazeGenerated[w,((z-1)+mazefloor)%mazefloor,x,y] = MazeGenerated[w,((z-1)+mazefloor)%mazefloor,x,y].Replace("B", "");
									break;
								case "B":
									MazeGenerated[w,z,x,y] = MazeGenerated[w,z,x,y].Replace("B", "");
									MazeGenerated[w,(z+1)%mazefloor,x,y] = MazeGenerated[w,(z+1)%mazefloor,x,y].Replace("T", "");
									break;
								case "F":
									MazeGenerated[w,z,x,y] = MazeGenerated[w,z,x,y].Replace("F", "");
									MazeGenerated[((w-1)+mazetimeline)%mazetimeline,z,x,y] = MazeGenerated[((w-1)+mazetimeline)%mazetimeline,z,x,y].Replace("P", "");
									break;
								case "P":
									MazeGenerated[w,z,x,y] = MazeGenerated[w,z,x,y].Replace("P", "");
									MazeGenerated[(w+1)%mazetimeline,z,x,y] = MazeGenerated[(w+1)%mazetimeline,z,x,y].Replace("F", "");
									break;
								default:
									break;
							}
						}
					}
				}
			}
			
			Debug.LogFormat("[Sickening Maze #{0}] < Maze Pointers Generated by the Module >", moduleId);
			for (int w = 0; w < mazetimeline; w++)
			{
				if (mazetimeline != 1)
				{
					Debug.LogFormat("[Sickening Maze #{0}] Connection Timeline {1}:", moduleId, w.ToString());
				}
				for (int z = 0; z < mazefloor; z++)
				{
					if (mazefloor != 1)
					{
						Debug.LogFormat("[Sickening Maze #{0}] Connection Floor {1}:", moduleId, z.ToString());
					}
					for (int x = 0; x < mazecolumn; x++)
					{
						string Mazery2 = "";
						for (int y = 0; y < mazerow; y++)
						{
							Mazery2 += ConnectionsGenerated[w,z,x,y];
						}
						Debug.LogFormat("[Sickening Maze #{0}] {1}", moduleId, Mazery2);
					}
					Debug.LogFormat("[Sickening Maze #{0}] ----------------------------------------------------", moduleId);
				}
			}
			
			Debug.LogFormat("[Sickening Maze #{0}] ----------------------------------------------------", moduleId);
			Debug.LogFormat("[Sickening Maze #{0}] < Maze Walls Generated by the Module >", moduleId);
			for (int w = 0; w < mazetimeline; w++)
			{
				if (mazetimeline != 1)
				{
					Debug.LogFormat("[Sickening Maze #{0}] Timeline {1}:", moduleId, w.ToString());
				}
				for (int z = 0; z < mazefloor; z++)
				{
					if (mazefloor != 1)
					{
						Debug.LogFormat("[Sickening Maze #{0}] Floor {1}:", moduleId, z.ToString());
					}
					for (int x = 0; x < mazecolumn; x++)
					{
						string Mazery = "";
						for (int y = 0; y < mazerow; y++)
						{
							Mazery += "[" + MazeGenerated[w,z,x,y] + "]";
						}
						Debug.LogFormat("[Sickening Maze #{0}] {1}", moduleId, Mazery);
					}
					Debug.LogFormat("[Sickening Maze #{0}] ----------------------------------------------------", moduleId);
				}
			}
			
		}
	}
	
	void KeyCoordinates()
	{
		Coordinates = new int[4] {0,0,0,0};
		Redo:
		int[,] GivenKeyCoordinates = new int[3,4];
		for (int x = 0; x < 3; x++)
		{
			for (int y = 0; y < 4; y++)
			{
				GivenKeyCoordinates[x,y] = UnityEngine.Random.Range(0,3);
			}
			
			if (x != 0)
			{
				for (int z = 0; z < x; z++)
				{
					int ManhattanDistance = 0, Placement = 0, PlacementTwo = 0;
					for (int y = 0; y < 4; y++)
					{
						ManhattanDistance = GivenKeyCoordinates[x,y] - GivenKeyCoordinates[z,y] < 0 ? ManhattanDistance + ((GivenKeyCoordinates[x,y] - GivenKeyCoordinates[z,y]) * -1) : ManhattanDistance + (GivenKeyCoordinates[x,y] - GivenKeyCoordinates[z,y]);
						Placement += GivenKeyCoordinates[x,y];
						PlacementTwo += GivenKeyCoordinates[z,y];
					}
					
					if (ManhattanDistance < 5 || Placement == 0 || Placement == 8 || PlacementTwo == 0 || PlacementTwo == 8)
					{
						goto Redo;
					}
				}
			}
		}
		
		for (int x = 0; x < 3; x++)
		{
			int DecimalConversion = 0;
			for (int y = 0; y < 4; y++)
			{
				DecimalConversion += Convert.ToInt32(GivenKeyCoordinates[x,y]*Math.Pow(3, 3-y));
			}
			KeyPlacementDecimal[x] = DecimalConversion;
		}
		
		Debug.LogFormat("[Sickening Maze #{0}] < Key Location Coordinates >", moduleId);
		for (int x = 0; x < 3; x++)
		{
			Debug.LogFormat("[Sickening Maze #{0}] Key Location {1}: ({2},{3},{4},{5})", moduleId, (x+1).ToString(), GivenKeyCoordinates[x,0], GivenKeyCoordinates[x,1], GivenKeyCoordinates[x,2], GivenKeyCoordinates[x,3]);
		}
		Debug.LogFormat("[Sickening Maze #{0}] ----------------------------------------------------", moduleId);
		Debug.LogFormat("[Sickening Maze #{0}] ----------------------------------------------------", moduleId);
	}

    void Sender()
    {
		if (Interactable)
		{
			if (Mode.text == "SEE")
			{
				Audio.PlaySoundAtTransform(SFX[15].name, transform);
				Mode.text = "SHUT";
				Movement.SetActive(false);
				Selection.SetActive(true);
				UpdateText();
			}
			
			else if (Mode.text == "SHUT")
			{
				Audio.PlaySoundAtTransform(SFX[15].name, transform);
				Mode.text = "SEE";
				Movement.SetActive(true);
				Selection.SetActive(false);
			}
			
			else if (Mode.text == "REDO")
			{
				Interactable = false;
				Debug.LogFormat("[Sickening Maze #{0}] You activated a multideminsional construct. Your current location is being reset to (0,0,0,0).", moduleId);
				StartCoroutine(InitiateWarpDrive());
			}
		}
    }
	
	void Check()
    {
		if (Interactable && Determinable)
		{
			if (Coordinates.Sum() == 8)
			{
				if (KeyPlacementGathered.Count() == 3)
				{
					Interactable = false;
					StartCoroutine(Freedom());
					Debug.LogFormat("[Sickening Maze #{0}] ----------------------------------------------------", moduleId);
					Debug.LogFormat("[Sickening Maze #{0}] This is the end of your path. Don't get lost again, traveller.", moduleId);
					Debug.LogFormat("[Sickening Maze #{0}] ----------------------------------------------------", moduleId);
				}
				
				else
				{
					Module.HandleStrike();
					KeyView.text = "E";
					StartCoroutine(DisplayValue(1));
					Debug.LogFormat("[Sickening Maze #{0}] You attempted to gather a key. There was no key in the current location.", moduleId);
				}
			}
				
			else
			{
				int DecimalConversion = 0;
				for (int x = 0; x < Coordinates.Length; x++)
				{
					DecimalConversion += Convert.ToInt32(Coordinates[x]*Math.Pow(3, 3-x));
				}
				
				if (KeyPlacementDecimal.Contains(DecimalConversion))
				{
					if (KeyPlacementGathered.ToArray().Contains(DecimalConversion))
					{
						KeyView.text = "A";
						StartCoroutine(DisplayValue(3));
						Debug.LogFormat("[Sickening Maze #{0}] You attempted to gather a key. There was a key here, but it has been collected.", moduleId);
					}
					
					else
					{
						KeyPlacementGathered.Add(DecimalConversion);
						KeyView.text = "B";
						StartCoroutine(DisplayValue(2));
						Debug.LogFormat("[Sickening Maze #{0}] You attempted to gather a key. It was successful. You added the key to your inventory.", moduleId);
					}
				}
				
				else
				{
					Module.HandleStrike();
					KeyView.text = "E";
					StartCoroutine(DisplayValue(1));
					Debug.LogFormat("[Sickening Maze #{0}] You attempted to gather a key. There was no key in the current location.", moduleId);
				}
			}
		}
    }
	
	void AlternateView(int TogglingNumber)
    {
		if (Interactable)
		{
			string LetterLayout = "ABCabc";
			Audio.PlaySoundAtTransform(SFX[16].name, transform);
			CurrentToggles[TogglingNumber] = (CurrentToggles[TogglingNumber] + 1)%3;
			ToggleText[TogglingNumber].text = LetterLayout[(3*TogglingNumber)+CurrentToggles[TogglingNumber]].ToString();
			UpdateText();
		}
    }
	
	void Moving(int Movement, int mazerow, int mazecolumn, int mazefloor, int mazetimeline)
	{
		if (Interactable)
		{
			Interactable = false;
			string PossibleWalls = "UDLRTBFP";
			string[] MovementOutcomes = {"went up", "went down", "went left", "went right", "went to the top", "went to the bottom", "went to the future", "went to the past"};
			if (MazeGenerated[Coordinates[0],Coordinates[1],Coordinates[2],Coordinates[3]].ToCharArray().Count(c => c == PossibleWalls[Movement]) == 0)
			{
				Alternator = (Alternator + 1) % 2;
				switch(PossibleWalls[Movement].ToString())
				{
					case "U":
						Coordinates[2] = ((Coordinates[2] - 1) + mazecolumn) % mazecolumn;
						break;
					case "D":
						Coordinates[2] = (Coordinates[2] + 1) % mazecolumn;
						break;
					case "L":
						Coordinates[3] = ((Coordinates[3] - 1) + mazerow) % mazerow;
						break;
					case "R":
						Coordinates[3] = (Coordinates[3] + 1) % mazerow;
						break;
					case "T":
						Coordinates[1] = ((Coordinates[1] - 1) + mazefloor) % mazefloor;
						break;
					case "B":
						Coordinates[1] = (Coordinates[1] + 1) % mazefloor;
						break;
					case "F":
						Coordinates[0] = ((Coordinates[0] - 1) + mazetimeline) % mazetimeline;
						break;
					case "P":
						Coordinates[0] = (Coordinates[0] + 1) % mazetimeline;
						break;
					default:
						break;
				}
				StartCoroutine(AcceptedMovement());
				Debug.LogFormat("[Sickening Maze #{0}] You {1}. The movement was possible. Current location is: ({2},{3},{4},{5}).", moduleId, MovementOutcomes[Movement], Coordinates[0], Coordinates[1], Coordinates[2], Coordinates[3]);
			}
			
			else
			{
				StartCoroutine(ForceFieldThud());
				Debug.LogFormat("[Sickening Maze #{0}] You {1}. A barrier was hit. Current location is: ({2},{3},{4},{5}).", moduleId, MovementOutcomes[Movement], Coordinates[0], Coordinates[1], Coordinates[2], Coordinates[3]);
			}
		}
	}
	
	void UpdateText()
	{
		Layout.text = "";
		for (int x = 0; x < 3; x++) // Column
		{
			for (int y = 0; y < 3; y++) // Row
			{
				int Converter = 0;
				for (int a = 0; a < 2; a++)
				{
					Converter += Convert.ToInt32(CurrentToggles[a]*Math.Pow(3, 3-a));
				}
				Converter = Converter + (3*x) + y;
				switch (ConnectionsGenerated[CurrentToggles[0],CurrentToggles[1],x,y].ToString())
				{
					case "U":
						Layout.text = KeyPlacementDecimal.Contains(Converter) ? Layout.text + "<color=#ff0000ff>@</color>" : Layout.text + "<color=#ff0000ff>8</color>";
						break;
					case "D":
						Layout.text = KeyPlacementDecimal.Contains(Converter) ? Layout.text + "<color=#ffff00ff>@</color>" : Layout.text + "<color=#ffff00ff>8</color>";
						break;
					case "L":
						Layout.text = KeyPlacementDecimal.Contains(Converter) ? Layout.text + "<color=#00ff00ff>@</color>" : Layout.text + "<color=#00ff00ff>8</color>";
						break;
					case "R":
						Layout.text = KeyPlacementDecimal.Contains(Converter) ? Layout.text + "<color=#ff00ffff>@</color>" : Layout.text + "<color=#ff00ffff>8</color>";
						break;
					case "T":
						Layout.text = KeyPlacementDecimal.Contains(Converter) ? Layout.text + "<color=#0000ffff>@</color>" : Layout.text + "<color=#0000ffff>8</color>";
						break;
					case "B":
						Layout.text = KeyPlacementDecimal.Contains(Converter) ? Layout.text + "<color=#00ffffff>@</color>" : Layout.text + "<color=#00ffffff>8</color>";
						break;
					case "F":
						Layout.text = KeyPlacementDecimal.Contains(Converter) ? Layout.text + "<color=#ffffffff>@</color>" : Layout.text + "<color=#ffffffff>8</color>";
						break;
					case "P":
						Layout.text = KeyPlacementDecimal.Contains(Converter) ? Layout.text + "<color=#808080ff>@</color>" : Layout.text + "<color=#808080ff>8</color>";
						break;
					case "X":
						Layout.text = KeyPlacementDecimal.Contains(Converter) ? Layout.text + "<color=#9f8170ff>@</color>" : Layout.text + "<color=#9f8170ff>8</color>";
						break;
					default:
						break;
				}
			}
			Layout.text = x < 2 ? Layout.text + "\n" : Layout.text;
		}
	}
	
	IEnumerator InitiateWarpDrive()
	{
		Movement.SetActive(false);
		Button.SetActive(false);
		Seedling.text = "";
		Alternator = 0;
		Digger.clip = SFX[5];
		Digger.Play();
		Color32 OldColor = Border.material.color;
		while (Digger.isPlaying)
		{
			Border.material.color = Color.Lerp(OldColor, new Color32(0,0,0,255), Digger.time / Digger.clip.length);
			yield return null;
		}
		Border.material.color = new Color32(0,0,0,255);
		yield return new WaitForSecondsRealtime(1f);
		Digger.clip = SFX[7+KeyPlacementGathered.Count()];
		Digger.Play();
		int NumberCheck = -1;
		while (Digger.isPlaying)
		{
			Border.material.color = Color.Lerp(new Color32(0,0,0,255), new Color32(0,0,255,255), Digger.time / Digger.clip.length);
			if ((int)Digger.time > NumberCheck)
			{
				Traveller.text = "";
				int ConversionToByte = Convert.ToInt32(255*((Digger.clip.length-Digger.time)/Digger.clip.length));
				string LetterConversion = "0123456789abcdef";
				string[] ValidColors = {"ff0000", "ffff00", "00ff00", "ff00ff", "0000ff", "00ffff", "ffffff", "808080"};
				for (int x = 0; x < 9; x++)
				{
					Traveller.text += "<color=#" + ValidColors[UnityEngine.Random.Range(0,ValidColors.Length)] + LetterConversion[ConversionToByte/16].ToString() + LetterConversion[ConversionToByte%16].ToString() + ">8</color>";
					if (x%3 == 2 && x < 8)
					{
						Traveller.text += "\n";
					}
				}
				NumberCheck++;
			}
			yield return null;
		}
		Border.material.color = new Color32(0,0,255,255);
		Traveller.text = "";
		yield return new WaitForSecondsRealtime(1f);
		Digger.clip = SFX[6];
		Digger.Play();
		Movement.SetActive(true);
		Button.SetActive(true);
		Coordinates = new int[4] {0,0,0,0};
		Seedling.text = "Uncertainty Awaits";
		KeyView.text = "";
		Mode.text = "SEE";
		Determinable = false;
		while (Digger.isPlaying)
		{
			yield return null;
		}
		Interactable = true;
	}
	
	IEnumerator DisplayValue(int Number)
	{
		Interactable = false;
		Determinable = false;
		Digger.clip = SFX[1+Number];
		Digger.Play();
		while (Digger.isPlaying)
		{
			yield return null;
		}
		Interactable = true;
	}
	
	IEnumerator ForceFieldThud()
    {
		Striking = true;
		Movement.SetActive(false);
		Button.SetActive(false);
		Seedling.text = "";
		Module.HandleStrike();
		Digger.clip = SFX[1];
		Digger.Play();
		Border.material.color = new Color32(255,255,255,255);
		while (Digger.isPlaying)
		{
			Border.material.color = Color.Lerp(new Color32(255,255,255,255), new Color32(0,0,Convert.ToByte(255/(Alternator+1)),255), Digger.time / Digger.clip.length);
			yield return null;
		}
		Border.material.color = new Color32(0,0,Convert.ToByte(255/(Alternator+1)),255);
		Interactable = true;
		Movement.SetActive(true);
		Button.SetActive(true);
		Seedling.text = "Uncertainty Awaits";
		Striking = false;
	}
	
	IEnumerator AcceptedMovement()
    {
		Travelling = true;
		Movement.SetActive(false);
		Button.SetActive(false);
		Seedling.text = "";
		Digger.clip = SFX[0];
		Digger.Play();
		Color32 OldColor = Border.material.color;
		while (Digger.isPlaying)
		{
			Border.material.color = Color.Lerp(OldColor, new Color32(0,0,Convert.ToByte(255/(Alternator+1)),255), Digger.time / Digger.clip.length);
			yield return null;
		}
		Border.material.color = new Color32(0,0,Convert.ToByte(255/(Alternator+1)),255);
		Interactable = true;
		Movement.SetActive(true);
		Button.SetActive(true);
		Seedling.text = "Uncertainty Awaits";
		KeyView.text = Coordinates.Sum() == 0 ? "" : "9";
		Mode.text = Coordinates.Sum() == 0 ? "SEE" : "REDO";
		Determinable = Coordinates.Sum() == 0 ? false : true;
		Travelling = false;
	}
	
	IEnumerator Freedom()
    {
		Movement.SetActive(false);
		Button.SetActive(false);
		Seedling.text = "";
		Audio.PlaySoundAtTransform(SFX[11].name, transform);
        yield return new WaitForSecondsRealtime(.75f);
		Digger.clip = SFX[12];
		Digger.Play();
		Color32 OldColor = Border.material.color;
		while (Digger.isPlaying)
		{
			Traveller.text = "";
			Border.material.color = Color.Lerp(OldColor, new Color32(255,0,0,255), Digger.time / Digger.clip.length);
			int ConversionToByte = Convert.ToInt32(255*((Digger.clip.length-Digger.time)/Digger.clip.length));
			string LetterConversion = "0123456789abcdef";
			string[] ValidColors = {"ff0000", "ffff00", "00ff00", "ff00ff", "0000ff", "00ffff", "ffffff", "808080"};
			for (int x = 0; x < 9; x++)
			{
				Traveller.text += "<color=#" + ValidColors[UnityEngine.Random.Range(0,ValidColors.Length)] + LetterConversion[ConversionToByte/16].ToString() + LetterConversion[ConversionToByte%16].ToString() + ">8</color>";
				if (x%3 == 2 && x < 8)
				{
					Traveller.text += "\n";
				}
			}
			yield return new WaitForSecondsRealtime(0.1f);
		}
		Traveller.text = "";
		Audio.PlaySoundAtTransform(SFX[13].name, transform);
		Border.material.color = new Color32(255,0,0,255);
		Module.HandlePass();
		ModuleSolved = true;
		yield return new WaitForSecondsRealtime(1.5f);
		string Scare = "BFDXZQKVHMSLIYENWROACPT2GU3J57";
		for (int x = 0; x < 8; x++)
		{
			Audio.PlaySoundAtTransform(SFX[14].name, transform);
			Continuation.text += Scare[UnityEngine.Random.Range(0,Scare.Length)].ToString();
			if (x%4 == 1)
			{
				for (int y = 0; y < 2; y++)
				{
					yield return new WaitForSecondsRealtime(0.125f);
					Continuation.text += " ";
					Audio.PlaySoundAtTransform(SFX[14].name, transform);
					
				}
			}
			if (x%4 == 3 && x < 7)
			{
				Continuation.text += "\n";
			}
			yield return new WaitForSecondsRealtime(0.125f);
		}
	}
	
	//twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"To move in the maze, use the command !{0} move [1 Letter Directions (Can be multiple: !{0} move UDLRTBFP)] | To check the current coordinates, use the command !{0} check | To press the top left button, use the command !{0} see/shut/redo | To toggle a dimensional coordinate during map view, use the command !{0} toggle [1-2 (in reading order)] (This command can be chained.)";
    #pragma warning restore 414
	
	IEnumerator ProcessTwitchCommand(string command)
	{
		string[] parameters = command.Split(' ');
		if (Regex.IsMatch(command, @"^\s*check\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			if (!Interactable)
			{
				yield return "sendtochaterror You can not interact with the module currently. The command was not processed.";
				yield break;
			}
			
			if (Mode.text != "REDO")
			{
				yield return "sendtochaterror You are in a position where you can't check for keys. The command was not processed.";
				yield break;
			}
			
			if (!Determinable)
			{
				yield return "sendtochaterror You can not check for keys again because you already checked it. The command was not processed.";
				yield break;
			}
			yield return "solve";
			Checker.OnInteract();
		}
		
		if (command.ToUpper().EqualsAny("SEE","SHUT","REDO"))
		{
			yield return null;
			if (!Interactable)
			{
				yield return "sendtochaterror You can not interact with the module currently. The command was not processed.";
				yield break;
			}
			
			if (command.ToUpper() != Mode.text)
			{
				yield return "sendtochaterror You can not interact with the button instance currently. The command was not processed.";
				yield break;
			}
			SendIt.OnInteract();
		}
		
		if (Regex.IsMatch(parameters[0], @"^\s*move\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			if (parameters.Length != 2)
			{
				yield return "sendtochaterror Invalid parameter length. The command was not processed.";
				yield break;
			}
			
			if (!Interactable)
			{
				yield return "sendtochaterror You can not interact with the module currently. The command was not processed.";
				yield break;
			}
			
			if (Mode.text == "SHUT")
			{
				yield return "sendtochaterror You can not move while checking the map. The command was not processed.";
				yield break;
			}
			
			string MovementAllowed = "UDLRTBFP";
			if (!parameters[1].ToUpper().ToCharArray().All(c => MovementAllowed.ToCharArray().Contains(c)))
			{
				yield return "sendtochaterror Movement sequence contain an invalid character. The command was not processed.";
				yield break;
			}
			
			for (int x = 0; x < parameters[1].Length; x++)
			{
				Arrows[Array.IndexOf(MovementAllowed.ToCharArray(), parameters[1].ToUpper()[x])].OnInteract();
				if (Striking)
				{
					yield return "strikemessage A barrier was hit during movement. The command was halted after " + (x+1).ToString() + " movements.";
				}
				while (Travelling)
				{
					yield return "trycancel The command was halted due to a cancel request. The command was halted after " + (x+1).ToString() + " movements.";
				}
			}
		}
		
		if (Regex.IsMatch(parameters[0], @"^\s*toggle\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			if (parameters.Length != 2)
			{
				yield return "sendtochaterror Invalid parameter length. The command was not processed.";
				yield break;
			}
			
			if (!Interactable)
			{
				yield return "sendtochaterror You can not interact with the module currently. The command was not processed.";
				yield break;
			}
			
			if (Mode.text != "SHUT")
			{
				yield return "sendtochaterror You are not viewing the map currently. The command was not processed.";
				yield break;
			}
			
			string NumbersAllowed = "12";
			if (!parameters[1].ToUpper().ToCharArray().All(c => NumbersAllowed.ToCharArray().Contains(c)))
			{
				yield return "sendtochaterror Toggle sequence contain an invalid character. The command was not processed.";
				yield break;
			}
			
			for (int x = 0; x < parameters[1].Length; x++)
			{
				Toggles[Array.IndexOf(NumbersAllowed.ToCharArray(), parameters[1].ToUpper()[x])].OnInteract();
				yield return new WaitForSecondsRealtime(0.1f);
			}
			
		}
	}
}
