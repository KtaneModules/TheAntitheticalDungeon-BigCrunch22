using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class SufferingMazeScript : MonoBehaviour
{
    public KMAudio Audio;
    public KMBombInfo Bomb;
    public KMBombModule Module;

    public AudioClip[] SFX;
	public AudioSource Digger;
	
	public GameObject Movement,Selection,Button;
	public MeshRenderer Border, Background;
	public KMSelectable[] Arrows, Toggles;
    public TextMesh Seedling, Layout, Mode, KeyView, Traveller;
	public TextMesh[] ToggleText;
    public KMSelectable SendIt, Checker;

	int[] Coordinates = new int[7];
	int[] KeyPlacementDecimal = new int[3];
	List<int> KeyPlacementGathered = new List<int>();
	
	string[,,,,,,] MazeGenerated = new string[8,8,8,8,8,8,8];
	string[,,,,,,] ConnectionsGenerated = new string[8,8,8,8,8,8,8];
	int[,,,,,,] RandomAlteration = new int[8,8,8,8,8,8,8];
	
	int MazeRow = 2, MazeColumn = 2, MazeFloor = 2, MazeTimeline = 2, MazeChance = 2, MazeInitial = 2, MazeCondition = 2; 
	int ForcedSteps = 50000, Alternator =  0;
	int[] CurrentToggles = {0,0,0,0};
	string TheBind = "BFDXZQKVHMSLIYENWROACPT-GU+J;:";

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
                Moving(Movement, MazeRow, MazeColumn, MazeFloor, MazeTimeline, MazeChance, MazeInitial, MazeCondition);
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
		TheBind = new string(TheBind.ToCharArray().Shuffle());
		Debug.LogFormat("[Suffering Maze #{0}] Seed Generated: {1}", moduleId, TheBind);
		Debug.LogFormat("[Suffering Maze #{0}] ----------------------------------------------------", moduleId);
		Debug.LogFormat("[Suffering Maze #{0}] ----------------------------------------------------", moduleId);
		Seedling.text = TheBind;
		Selection.SetActive(false);
        KeyCoordinates();
		GenerateMazeOriginShift(MazeRow, MazeColumn, MazeFloor, MazeTimeline, MazeChance, MazeInitial, MazeCondition);
    }
	
	void GenerateMazeOriginShift(int mazerow, int mazecolumn, int mazefloor, int mazetimeline, int mazechance, int mazeinitial, int mazecondition)
	{
		if (mazerow > 0 && mazecolumn > 0  && mazefloor > 0 && mazetimeline > 0 && mazechance > 0 && mazeinitial > 0 && mazecondition > 0 && (mazerow + mazecolumn + mazefloor + mazetimeline + mazechance + mazeinitial + mazecondition) > 7)
		{
			MazeGenerated = new string[mazecondition,mazeinitial,mazechance,mazetimeline,mazefloor,mazecolumn,mazerow];
			ConnectionsGenerated = new string[mazecondition,mazeinitial,mazechance,mazetimeline,mazefloor,mazecolumn,mazerow];
			RandomAlteration = new int[mazecondition,mazeinitial,mazechance,mazetimeline,mazefloor,mazecolumn,mazerow];
			for (int t = 0; t < mazecondition; t++)
			{
				for (int u = 0; u < mazeinitial; u++)
				{
					for (int v = 0; v < mazechance; v++)
					{
						for (int w = 0; w < mazetimeline; w++)
						{
							for (int z = 0; z < mazefloor; z++)
							{
								for (int x = 0; x < mazecolumn; x++)
								{
									for (int y = 0; y < mazerow; y++)
									{
										//U = Up, D = Down, L = Left, R = Right, T = Top, B = Bottom, F = Future, P = Past, O = Outcome, C = Cause, H = Change, E = Revert, A = Ascend, S = Descend
										MazeGenerated[t,u,v,w,z,x,y] = "UDLRTBFPOCHEAS";
										RandomAlteration[t,u,v,w,z,x,y] = UnityEngine.Random.Range(0,2);
										if (y < mazerow - 1)
										{
											ConnectionsGenerated[t,u,v,w,z,x,y] = "R";
										}
										
										else
										{
											if (x < mazecolumn - 1)
											{
												ConnectionsGenerated[t,u,v,w,z,x,y] = "D";
											}
											
											else
											{
												if (z < mazefloor - 1)
												{
													ConnectionsGenerated[t,u,v,w,z,x,y] = "B";
												}
												
												else
												{
													if (w < mazetimeline - 1)
													{
														ConnectionsGenerated[t,u,v,w,z,x,y] = "P";
													}
													
													else
													{
														if (v < mazechance - 1)
														{
															ConnectionsGenerated[t,u,v,w,z,x,y] = "C";
														}
														
														else
														{
															if (u < mazeinitial - 1)
															{
																ConnectionsGenerated[t,u,v,w,z,x,y] = "E";
															}
															
															else
															{
																if (t < mazecondition - 1)
																{
																	ConnectionsGenerated[t,u,v,w,z,x,y] = "S";
																}
																
																else
																{
																	ConnectionsGenerated[t,u,v,w,z,x,y] = "X";
																}
															}
														}
													}
												}
											}
										}
										
									}
								}
							}
						}
					}
				}
			}
			
			int CurrentRow = mazerow - 1, CurrentColumn = mazecolumn - 1, CurrentFloor = mazefloor - 1, CurrentTimeline = mazetimeline - 1, CurrentChance = mazechance - 1, CurrentInitial = mazeinitial - 1, CurrentCondition = mazecondition - 1;
			for (int x = 0; x < ForcedSteps; x++)
			{
				bool StepValid = false;
				do
				{
					int Movement = UnityEngine.Random.Range(0,14);
					switch(Movement) // Remove the "if" statement if you want to generate a looping maze.
					{
						case 0:
							//if (CurrentColumn != 0)
							//{
								ConnectionsGenerated[CurrentCondition,CurrentInitial,CurrentChance,CurrentTimeline,CurrentFloor,CurrentColumn,CurrentRow] = "U";
								CurrentColumn = ((CurrentColumn - 1) + mazecolumn) % mazecolumn;
								ConnectionsGenerated[CurrentCondition,CurrentInitial,CurrentChance,CurrentTimeline,CurrentFloor,CurrentColumn,CurrentRow] = "X";
								StepValid = true;
							//}
							break;
						case 1:
							//if (CurrentColumn != mazecolumn - 1)
							//{
								ConnectionsGenerated[CurrentCondition,CurrentInitial,CurrentChance,CurrentTimeline,CurrentFloor,CurrentColumn,CurrentRow] = "D";
								CurrentColumn = (CurrentColumn + 1) % mazecolumn;
								ConnectionsGenerated[CurrentCondition,CurrentInitial,CurrentChance,CurrentTimeline,CurrentFloor,CurrentColumn,CurrentRow] = "X";
								StepValid = true;
							//}
							
							break;
						case 2:
							//if (CurrentRow != 0)
							//{
								ConnectionsGenerated[CurrentCondition,CurrentInitial,CurrentChance,CurrentTimeline,CurrentFloor,CurrentColumn,CurrentRow] = "L";
								CurrentRow = ((CurrentRow - 1) + mazerow) % mazerow;
								ConnectionsGenerated[CurrentCondition,CurrentInitial,CurrentChance,CurrentTimeline,CurrentFloor,CurrentColumn,CurrentRow] = "X";
								StepValid = true;
							//}
							break;
						case 3:
							//if (CurrentRow != mazerow - 1)
							//{
								ConnectionsGenerated[CurrentCondition,CurrentInitial,CurrentChance,CurrentTimeline,CurrentFloor,CurrentColumn,CurrentRow] = "R";
								CurrentRow = (CurrentRow + 1) % mazerow;
								ConnectionsGenerated[CurrentCondition,CurrentInitial,CurrentChance,CurrentTimeline,CurrentFloor,CurrentColumn,CurrentRow] = "X";
								StepValid = true;
							//}
							break;
						case 4:
							//if (CurrentFloor != 0)
							//{
								ConnectionsGenerated[CurrentCondition,CurrentInitial,CurrentChance,CurrentTimeline,CurrentFloor,CurrentColumn,CurrentRow] = "T";
								CurrentFloor = ((CurrentFloor - 1) + mazefloor) % mazefloor;
								ConnectionsGenerated[CurrentCondition,CurrentInitial,CurrentChance,CurrentTimeline,CurrentFloor,CurrentColumn,CurrentRow] = "X";
								StepValid = true;
							//}
							break;
						case 5:
							//if (CurrentFloor != mazefloor - 1)
							//{
								ConnectionsGenerated[CurrentCondition,CurrentInitial,CurrentChance,CurrentTimeline,CurrentFloor,CurrentColumn,CurrentRow] = "B";
								CurrentFloor = (CurrentFloor + 1) % mazefloor;
								ConnectionsGenerated[CurrentCondition,CurrentInitial,CurrentChance,CurrentTimeline,CurrentFloor,CurrentColumn,CurrentRow] = "X";
								StepValid = true;
							//}
							break;
						case 6:
							//if (CurrentTimeline != 0)
							//{
								ConnectionsGenerated[CurrentCondition,CurrentInitial,CurrentChance,CurrentTimeline,CurrentFloor,CurrentColumn,CurrentRow] = "F";
								CurrentTimeline = ((CurrentTimeline - 1) + mazetimeline) % mazetimeline;
								ConnectionsGenerated[CurrentCondition,CurrentInitial,CurrentChance,CurrentTimeline,CurrentFloor,CurrentColumn,CurrentRow] = "X";
								StepValid = true;
							//}
							break;
						case 7:
							//if (CurrentTimeline != mazetimeline - 1)
							//{
								ConnectionsGenerated[CurrentCondition,CurrentInitial,CurrentChance,CurrentTimeline,CurrentFloor,CurrentColumn,CurrentRow] = "P";
								CurrentTimeline = (CurrentTimeline + 1) % mazetimeline;
								ConnectionsGenerated[CurrentCondition,CurrentInitial,CurrentChance,CurrentTimeline,CurrentFloor,CurrentColumn,CurrentRow] = "X";
								StepValid = true;
							//}
							break;
						case 8:
							//if (CurrentChance != 0)
							//{
								ConnectionsGenerated[CurrentCondition,CurrentInitial,CurrentChance,CurrentTimeline,CurrentFloor,CurrentColumn,CurrentRow] = "O";
								CurrentChance = ((CurrentChance - 1) + mazechance) % mazechance;
								ConnectionsGenerated[CurrentCondition,CurrentInitial,CurrentChance,CurrentTimeline,CurrentFloor,CurrentColumn,CurrentRow] = "X";
								StepValid = true;
							//}
							break;
						case 9:
							//if (CurrentChance != mazechance - 1)
							//{
								ConnectionsGenerated[CurrentCondition,CurrentInitial,CurrentChance,CurrentTimeline,CurrentFloor,CurrentColumn,CurrentRow] = "C";
								CurrentChance = (CurrentChance + 1) % mazechance;
								ConnectionsGenerated[CurrentCondition,CurrentInitial,CurrentChance,CurrentTimeline,CurrentFloor,CurrentColumn,CurrentRow] = "X";
								StepValid = true;
							//}
							break;
						case 10:
							//if (CurrentInitial != 0)
							//{
								ConnectionsGenerated[CurrentCondition,CurrentInitial,CurrentChance,CurrentTimeline,CurrentFloor,CurrentColumn,CurrentRow] = "H";
								CurrentInitial = ((CurrentInitial - 1) + mazeinitial) % mazeinitial;
								ConnectionsGenerated[CurrentCondition,CurrentInitial,CurrentChance,CurrentTimeline,CurrentFloor,CurrentColumn,CurrentRow] = "X";
								StepValid = true;
							//}
							break;
						case 11:
							//if (CurrentInitial != mazeinitial - 1)
							//{
								ConnectionsGenerated[CurrentCondition,CurrentInitial,CurrentChance,CurrentTimeline,CurrentFloor,CurrentColumn,CurrentRow] = "E";
								CurrentInitial = (CurrentInitial + 1) % mazeinitial;
								ConnectionsGenerated[CurrentCondition,CurrentInitial,CurrentChance,CurrentTimeline,CurrentFloor,CurrentColumn,CurrentRow] = "X";
								StepValid = true;
							//}
							break;
						case 12:
							//if (CurrentCondition != 0)
							//{
								ConnectionsGenerated[CurrentCondition,CurrentInitial,CurrentChance,CurrentTimeline,CurrentFloor,CurrentColumn,CurrentRow] = "A";
								CurrentCondition = ((CurrentCondition - 1) + mazecondition) % mazecondition;
								ConnectionsGenerated[CurrentCondition,CurrentInitial,CurrentChance,CurrentTimeline,CurrentFloor,CurrentColumn,CurrentRow] = "X";
								StepValid = true;
							//}
							break;
						case 13:
							//if (CurrentCondition != mazecondition - 1)
							//{
								ConnectionsGenerated[CurrentCondition,CurrentInitial,CurrentChance,CurrentTimeline,CurrentFloor,CurrentColumn,CurrentRow] = "S";
								CurrentCondition = (CurrentCondition + 1) % mazecondition;
								ConnectionsGenerated[CurrentCondition,CurrentInitial,CurrentChance,CurrentTimeline,CurrentFloor,CurrentColumn,CurrentRow] = "X";
								StepValid = true;
							//}
							break;
						default:
							break;
					}
				}
				while (!StepValid);
			}
			
			for (int t = 0; t < mazecondition; t++)
			{
				for (int u = 0; u < mazeinitial; u++)
				{
					for (int v = 0; v < mazechance; v++)
					{
						for (int w = 0; w < mazetimeline; w++)
						{
							for (int z = 0; z < mazefloor; z++)
							{
								for (int x = 0; x < mazecolumn; x++)
								{
									for (int y = 0; y < mazerow; y++)
									{
										switch(ConnectionsGenerated[t,u,v,w,z,x,y])
										{
											case "U":
												MazeGenerated[t,u,v,w,z,x,y] = MazeGenerated[t,u,v,w,z,x,y].Replace("U", "");
												MazeGenerated[t,u,v,w,z,((x-1)+mazecolumn)%mazecolumn,y] = MazeGenerated[t,u,v,w,z,((x-1)+mazecolumn)%mazecolumn,y].Replace("D", "");
												break;
											case "D":
												MazeGenerated[t,u,v,w,z,x,y] = MazeGenerated[t,u,v,w,z,x,y].Replace("D", "");
												MazeGenerated[t,u,v,w,z,(x+1)%mazecolumn,y] = MazeGenerated[t,u,v,w,z,(x+1)%mazecolumn,y].Replace("U", "");
												break;
											case "L":
												MazeGenerated[t,u,v,w,z,x,y] = MazeGenerated[t,u,v,w,z,x,y].Replace("L", "");
												MazeGenerated[t,u,v,w,z,x,((y-1)+mazerow)%mazerow] = MazeGenerated[t,u,v,w,z,x,((y-1)+mazerow)%mazerow].Replace("R", "");
												break;
											case "R":
												MazeGenerated[t,u,v,w,z,x,y] = MazeGenerated[t,u,v,w,z,x,y].Replace("R", "");
												MazeGenerated[t,u,v,w,z,x,(y+1)%mazerow] = MazeGenerated[t,u,v,w,z,x,(y+1)%mazerow].Replace("L", "");
												break;
											case "T":
												MazeGenerated[t,u,v,w,z,x,y] = MazeGenerated[t,u,v,w,z,x,y].Replace("T", "");
												MazeGenerated[t,u,v,w,((z-1)+mazefloor)%mazefloor,x,y] = MazeGenerated[t,u,v,w,((z-1)+mazefloor)%mazefloor,x,y].Replace("B", "");
												break;
											case "B":
												MazeGenerated[t,u,v,w,z,x,y] = MazeGenerated[t,u,v,w,z,x,y].Replace("B", "");
												MazeGenerated[t,u,v,w,(z+1)%mazefloor,x,y] = MazeGenerated[t,u,v,w,(z+1)%mazefloor,x,y].Replace("T", "");
												break;
											case "F":
												MazeGenerated[t,u,v,w,z,x,y] = MazeGenerated[t,u,v,w,z,x,y].Replace("F", "");
												MazeGenerated[t,u,v,((w-1)+mazetimeline)%mazetimeline,z,x,y] = MazeGenerated[t,u,v,((w-1)+mazetimeline)%mazetimeline,z,x,y].Replace("P", "");
												break;
											case "P":
												MazeGenerated[t,u,v,w,z,x,y] = MazeGenerated[t,u,v,w,z,x,y].Replace("P", "");
												MazeGenerated[t,u,v,(w+1)%mazetimeline,z,x,y] = MazeGenerated[t,u,v,(w+1)%mazetimeline,z,x,y].Replace("F", "");
												break;
											case "O":
												MazeGenerated[t,u,v,w,z,x,y] = MazeGenerated[t,u,v,w,z,x,y].Replace("O", "");
												MazeGenerated[t,u,((v-1)+mazechance)%mazechance,w,z,x,y] = MazeGenerated[t,u,((v-1)+mazechance)%mazechance,w,z,x,y].Replace("C", "");
												break;
											case "C":
												MazeGenerated[t,u,v,w,z,x,y] = MazeGenerated[t,u,v,w,z,x,y].Replace("C", "");
												MazeGenerated[t,u,(v+1)%mazechance,w,z,x,y] = MazeGenerated[t,u,(v+1)%mazechance,w,z,x,y].Replace("O", "");
												break;
											case "H":
												MazeGenerated[t,u,v,w,z,x,y] = MazeGenerated[t,u,v,w,z,x,y].Replace("H", "");
												MazeGenerated[t,((u-1)+mazeinitial)%mazeinitial,v,w,z,x,y] = MazeGenerated[t,((u-1)+mazeinitial)%mazeinitial,v,w,z,x,y].Replace("E", "");
												break;
											case "E":
												MazeGenerated[t,u,v,w,z,x,y] = MazeGenerated[t,u,v,w,z,x,y].Replace("E", "");
												MazeGenerated[t,(u+1)%mazeinitial,v,w,z,x,y] = MazeGenerated[t,(u+1)%mazeinitial,v,w,z,x,y].Replace("H", "");
												break;
											case "A":
												MazeGenerated[t,u,v,w,z,x,y] = MazeGenerated[t,u,v,w,z,x,y].Replace("A", "");
												MazeGenerated[((t-1)+mazecondition)%mazecondition,u,v,w,z,x,y] = MazeGenerated[((t-1)+mazecondition)%mazecondition,u,v,w,z,x,y].Replace("S", "");
												break;
											case "S":
												MazeGenerated[t,u,v,w,z,x,y] = MazeGenerated[t,u,v,w,z,x,y].Replace("S", "");
												MazeGenerated[(t+1)%mazecondition,u,v,w,z,x,y] = MazeGenerated[(t+1)%mazecondition,u,v,w,z,x,y].Replace("A", "");
												break;
											default:
												break;
										}
									}
								}
							}
						}
					}
				}
			}

			Debug.LogFormat("[Suffering Maze #{0}] < Maze Pointers Generated by the Module >", moduleId);
			for (int t = 0; t < mazecondition; t++)
			{
				if (mazecondition != 1)
				{
					Debug.LogFormat("[Suffering Maze #{0}] Connection Condition {1}:", moduleId, t.ToString());
				}
				for (int u = 0; u < mazeinitial; u++)
				{
					if (mazeinitial != 1)
					{
						Debug.LogFormat("[Suffering Maze #{0}] Connection Scenario {1}:", moduleId, u.ToString());
					}
					for (int v = 0; v < mazechance; v++)
					{
						if (mazechance != 1)
						{
							Debug.LogFormat("[Suffering Maze #{0}] Connection Probability {1}:", moduleId, v.ToString());
						}
						for (int w = 0; w < mazetimeline; w++)
						{
							if (mazetimeline != 1)
							{
								Debug.LogFormat("[Suffering Maze #{0}] Connection Timeline {1}:", moduleId, w.ToString());
							}
							for (int z = 0; z < mazefloor; z++)
							{
								if (mazefloor != 1)
								{
									Debug.LogFormat("[Suffering Maze #{0}] Connection Floor {1}:", moduleId, z.ToString());
								}
								for (int x = 0; x < mazecolumn; x++)
								{
									string Mazery2 = "";
									for (int y = 0; y < mazerow; y++)
									{
										Mazery2 += ConnectionsGenerated[t,u,v,w,z,x,y];
									}
									Debug.LogFormat("[Suffering Maze #{0}] {1}", moduleId, Mazery2);
								}
								Debug.LogFormat("[Suffering Maze #{0}] ----------------------------------------------------", moduleId);
							}
						}
					}
				}
			}
			
			Debug.LogFormat("[Suffering Maze #{0}] ----------------------------------------------------", moduleId);
			Debug.LogFormat("[Suffering Maze #{0}] < Maze Walls Generated by the Module >", moduleId);
			for (int t = 0; t < mazecondition; t++)
			{
				if (mazecondition != 1)
				{
					Debug.LogFormat("[Suffering Maze #{0}] Condition {1}:", moduleId, t.ToString());
				}
				for (int u = 0; u < mazeinitial; u++)
				{
					if (mazeinitial != 1)
					{
						Debug.LogFormat("[Suffering Maze #{0}] Scenario {1}:", moduleId, u.ToString());
					}
					for (int v = 0; v < mazechance; v++)
					{
						if (mazechance != 1)
						{
							Debug.LogFormat("[Suffering Maze #{0}] Probability {1}:", moduleId, v.ToString());
						}
						for (int w = 0; w < mazetimeline; w++)
						{
							if (mazetimeline != 1)
							{
								Debug.LogFormat("[Suffering Maze #{0}] Timeline {1}:", moduleId, w.ToString());
							}
							for (int z = 0; z < mazefloor; z++)
							{
								if (mazefloor != 1)
								{
									Debug.LogFormat("[Suffering Maze #{0}] Floor {1}:", moduleId, z.ToString());
								}
								for (int x = 0; x < mazecolumn; x++)
								{
									string Mazery = "";
									for (int y = 0; y < mazerow; y++)
									{
										Mazery += "[" + MazeGenerated[t,u,v,w,z,x,y] + "]";
									}
									Debug.LogFormat("[Suffering Maze #{0}] {1}", moduleId, Mazery);
								}
								Debug.LogFormat("[Suffering Maze #{0}] ----------------------------------------------------", moduleId);
							}
						}
					}
				}
			}
		}
	}
	
	void KeyCoordinates()
	{
		Coordinates = new int[7] {0,0,0,0,0,0,0};
		Redo:
		int[,] GivenKeyCoordinates = new int[3,7];
		for (int x = 0; x < 3; x++)
		{
			for (int y = 0; y < 7; y++)
			{
				GivenKeyCoordinates[x,y] = UnityEngine.Random.Range(0,2);
			}
			
			if (x != 0)
			{
				for (int z = 0; z < x; z++)
				{
					int ManhattanDistance = 0, Placement = 0, PlacementTwo = 0;
					for (int y = 0; y < 7; y++)
					{
						ManhattanDistance = GivenKeyCoordinates[x,y] - GivenKeyCoordinates[z,y] < 0 ? ManhattanDistance + ((GivenKeyCoordinates[x,y] - GivenKeyCoordinates[z,y]) * -1) : ManhattanDistance + (GivenKeyCoordinates[x,y] - GivenKeyCoordinates[z,y]);
						Placement += GivenKeyCoordinates[x,y];
						PlacementTwo += GivenKeyCoordinates[z,y];
					}
					
					if (ManhattanDistance < 4 || Placement == 0 || Placement == 7 || PlacementTwo == 0 || PlacementTwo == 7)
					{
						goto Redo;
					}
				}
			}
		}
		
		for (int x = 0; x < 3; x++)
		{
			int DecimalConversion = 0;
			for (int y = 0; y < 7; y++)
			{
				DecimalConversion += Convert.ToInt32(GivenKeyCoordinates[x,y]*Math.Pow(2, 6-y));
			}
			KeyPlacementDecimal[x] = DecimalConversion;
		}
		
		Debug.LogFormat("[Suffering Maze #{0}] < Key Location Coordinates >", moduleId);
		for (int x = 0; x < 3; x++)
		{
			Debug.LogFormat("[Suffering Maze #{0}] Key Location {1}: ({2},{3},{4},{5},{6},{7},{8})", moduleId, (x+1).ToString(), GivenKeyCoordinates[x,0], GivenKeyCoordinates[x,1], GivenKeyCoordinates[x,2], GivenKeyCoordinates[x,3], GivenKeyCoordinates[x,4], GivenKeyCoordinates[x,5], GivenKeyCoordinates[x,6]);
		}
		Debug.LogFormat("[Suffering Maze #{0}] ----------------------------------------------------", moduleId);
		Debug.LogFormat("[Suffering Maze #{0}] ----------------------------------------------------", moduleId);
	}

    void Sender()
    {
		if (Interactable)
		{
			if (Mode.text == "SEE")
			{
				Audio.PlaySoundAtTransform(SFX[16].name, transform);
				Mode.text = "SHUT";
				Movement.SetActive(false);
				Selection.SetActive(true);
				UpdateText();
			}
			
			else if (Mode.text == "SHUT")
			{
				Audio.PlaySoundAtTransform(SFX[16].name, transform);
				Mode.text = "SEE";
				Movement.SetActive(true);
				Selection.SetActive(false);
			}
			
			else if (Mode.text == "REDO")
			{
				Interactable = false;
				Debug.LogFormat("[Suffering Maze #{0}] You activated a multideminsional construct. Your current location is being reset to (0,0,0,0,0,0,0).", moduleId);
				StartCoroutine(InitiateWarpDrive());
			}
		}
    }
	
	void Check()
    {
		if (Interactable && Determinable)
		{
			if (Coordinates.Sum() == 7)
			{
				if (KeyPlacementGathered.Count() == 3)
				{
					Interactable = false;
					StartCoroutine(Freedom());
					Debug.LogFormat("[Suffering Maze #{0}] ----------------------------------------------------", moduleId);
					Debug.LogFormat("[Suffering Maze #{0}] I bid you farewell, traveler.", moduleId);
					Debug.LogFormat("[Suffering Maze #{0}] ----------------------------------------------------", moduleId);
				}
				
				else
				{
					Module.HandleStrike();
					KeyView.text = "4";
					StartCoroutine(DisplayValue(1));
					Debug.LogFormat("[Suffering Maze #{0}] You attempted to gather a key. There was no key in the current location.", moduleId);
				}
			}
				
			else
			{
				int DecimalConversion = 0;
				for (int x = 0; x < Coordinates.Length; x++)
				{
					DecimalConversion += Convert.ToInt32(Coordinates[x]*Math.Pow(2, 6-x));
				}
				
				if (KeyPlacementDecimal.Contains(DecimalConversion))
				{
					if (KeyPlacementGathered.ToArray().Contains(DecimalConversion))
					{
						KeyView.text = "6";
						StartCoroutine(DisplayValue(3));
						Debug.LogFormat("[Suffering Maze #{0}] You attempted to gather a key. There was a key here, but it has been collected.", moduleId);
					}
					
					else
					{
						KeyPlacementGathered.Add(DecimalConversion);
						KeyView.text = "0";
						StartCoroutine(DisplayValue(2));
						Debug.LogFormat("[Suffering Maze #{0}] You attempted to gather a key. It was successful. You added the key to your inventory.", moduleId);
					}
				}
				
				else
				{
					Module.HandleStrike();
					KeyView.text = "4";
					StartCoroutine(DisplayValue(1));
					Debug.LogFormat("[Suffering Maze #{0}] You attempted to gather a key. There was no key in the current location.", moduleId);
				}
			}
		}
    }
	
	void AlternateView(int TogglingNumber)
    {
		if (Interactable)
		{
			Audio.PlaySoundAtTransform(SFX[17].name, transform);
			CurrentToggles[TogglingNumber] = (CurrentToggles[TogglingNumber] + 1)%2;
			ToggleText[TogglingNumber].text = (2*(3-TogglingNumber) + CurrentToggles[TogglingNumber]).ToString();
			UpdateText();
		}
    }
	
	void Moving(int Movement, int mazerow, int mazecolumn, int mazefloor, int mazetimeline, int mazechance, int mazeinitial, int mazecondition)
	{
		if (Interactable)
		{
			Interactable = false;
			string PossibleWalls = "UDLRTBFPOCHEAS";
			string[] MovementOutcomes = {"went up", "went down", "went left", "went right", "went to the top", "went to the bottom", "went to the future", "went to the past", "went to the outcome", "went to the cause", "performed a change", "performed a reversion", "ascended", "descended"};
			if (MazeGenerated[Coordinates[0],Coordinates[1],Coordinates[2],Coordinates[3],Coordinates[4],Coordinates[5],Coordinates[6]].ToCharArray().Count(c => c == PossibleWalls[Movement]) == 0)
			{
				Alternator = (Alternator + 1) % 2;
				switch(PossibleWalls[Movement].ToString())
				{
					case "U":
						Coordinates[5] = ((Coordinates[5] - 1) + mazecolumn) % mazecolumn;
						break;
					case "D":
						Coordinates[5] = (Coordinates[5] + 1) % mazecolumn;
						break;
					case "L":
						Coordinates[6] = ((Coordinates[6] - 1) + mazerow) % mazerow;
						break;
					case "R":
						Coordinates[6] = (Coordinates[6] + 1) % mazerow;
						break;
					case "T":
						Coordinates[4] = ((Coordinates[4] - 1) + mazefloor) % mazefloor;
						break;
					case "B":
						Coordinates[4] = (Coordinates[4] + 1) % mazefloor;
						break;
					case "F":
						Coordinates[3] = ((Coordinates[3] - 1) + mazetimeline) % mazetimeline;
						break;
					case "P":
						Coordinates[3] = (Coordinates[3] + 1) % mazetimeline;
						break;
					case "O":
						Coordinates[2] = ((Coordinates[2] - 1) + mazechance) % mazechance;
						break;
					case "C":
						Coordinates[2] = (Coordinates[2] + 1) % mazechance;
						break;
					case "H":
						Coordinates[1] = ((Coordinates[1] - 1) + mazeinitial) % mazeinitial;
						break;
					case "E":
						Coordinates[1] = (Coordinates[1] + 1) % mazeinitial;
						break;
					case "A":
						Coordinates[0] = ((Coordinates[0] - 1) + mazecondition) % mazecondition;
						break;
					case "S":
						Coordinates[0] = (Coordinates[0] + 1) % mazecondition;
						break;
					default:
						break;
				}
				StartCoroutine(AcceptedMovement());
				Debug.LogFormat("[Suffering Maze #{0}] You {1}. The movement was possible. Current location is: ({2},{3},{4},{5},{6},{7},{8}).", moduleId, MovementOutcomes[Movement], Coordinates[0], Coordinates[1], Coordinates[2], Coordinates[3], Coordinates[4], Coordinates[5], Coordinates[6]);
			}
			
			else
			{
				StartCoroutine(ForceFieldThud());
				Debug.LogFormat("[Suffering Maze #{0}] You {1}. A barrier was hit. Current location is: ({2},{3},{4},{5},{6},{7},{8}).", moduleId, MovementOutcomes[Movement], Coordinates[0], Coordinates[1], Coordinates[2], Coordinates[3], Coordinates[4], Coordinates[5], Coordinates[6]);
			}
		}
	}
	
	void UpdateText()
	{
		Layout.text = "";
		string Baseline = "UDLRTBFPOCHEASX";
		for (int x = 0; x < 2; x++) // Column
		{
			for (int z = 0; z < 2; z++) // Floor
			{
				for (int y = 0; y < 2; y++) // Row
				{
					int Converter = 0;
					for (int a = 0; a < 4; a++)
					{
						Converter += Convert.ToInt32(CurrentToggles[a]*Math.Pow(2, 6-a));
					}
					Converter = Converter + (4*z) + (2*x) + y;
					Layout.text = KeyPlacementDecimal.Contains(Converter) ? Layout.text + "<color=#800000ff>" + TheBind[(15*RandomAlteration[CurrentToggles[0],CurrentToggles[1],CurrentToggles[2],CurrentToggles[3],z,x,y])+Baseline.IndexOf(ConnectionsGenerated[CurrentToggles[0],CurrentToggles[1],CurrentToggles[2],CurrentToggles[3],z,x,y])].ToString() + "</color>" : Layout.text + TheBind[(15*RandomAlteration[CurrentToggles[0],CurrentToggles[1],CurrentToggles[2],CurrentToggles[3],z,x,y])+Baseline.IndexOf(ConnectionsGenerated[CurrentToggles[0],CurrentToggles[1],CurrentToggles[2],CurrentToggles[3],z,x,y])].ToString();
				}
				Layout.text = z == 0 ? Layout.text + "  " : Layout.text;
			}
			Layout.text = x == 0 ? Layout.text + "\n" : Layout.text;
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
		Traveller.color = new Color32(255,255,255,255);
		while (Digger.isPlaying)
		{
			Border.material.color = Color.Lerp(new Color32(0,0,0,255), new Color32(255,0,0,255), Digger.time / Digger.clip.length);
			Traveller.color = Color.Lerp(new Color32(255,255,255,255), new Color32(0,0,0,255), Digger.time / Digger.clip.length);
			if ((int)Digger.time > NumberCheck)
			{
				NumberCheck++;
				Traveller.text = TheBind[UnityEngine.Random.Range(0,TheBind.Length)].ToString() + TheBind[UnityEngine.Random.Range(0,TheBind.Length)].ToString() + "  " + TheBind[UnityEngine.Random.Range(0,TheBind.Length)].ToString() + TheBind[UnityEngine.Random.Range(0,TheBind.Length)].ToString() + "\n" + TheBind[UnityEngine.Random.Range(0,TheBind.Length)].ToString() + TheBind[UnityEngine.Random.Range(0,TheBind.Length)].ToString() + "  " + TheBind[UnityEngine.Random.Range(0,TheBind.Length)].ToString() + TheBind[UnityEngine.Random.Range(0,TheBind.Length)].ToString();
			}
			yield return null;
		}
		Border.material.color = new Color32(255,0,0,255);
		Traveller.color = new Color32(0,0,0,255);
		yield return new WaitForSecondsRealtime(1f);
		Digger.clip = SFX[6];
		Digger.Play();
		Movement.SetActive(true);
		Button.SetActive(true);
		Coordinates = new int[7] {0,0,0,0,0,0,0};
		Seedling.text = TheBind;
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
			Border.material.color = Color.Lerp(new Color32(255,255,255,255), new Color32(Convert.ToByte(255/(Alternator+1)),0,0,255), Digger.time / Digger.clip.length);
			yield return null;
		}
		Border.material.color = new Color32(Convert.ToByte(255/(Alternator+1)),0,0,255);
		Interactable = true;
		Movement.SetActive(true);
		Button.SetActive(true);
		Seedling.text = Coordinates.Sum() == 0 ? TheBind : "Uncertainty Awaits";
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
			Border.material.color = Color.Lerp(OldColor, new Color32(Convert.ToByte(255/(Alternator+1)),0,0,255), Digger.time / Digger.clip.length);
			yield return null;
		}
		Border.material.color = new Color32(Convert.ToByte(255/(Alternator+1)),0,0,255);
		Interactable = true;
		Movement.SetActive(true);
		Button.SetActive(true);
		Seedling.text = Coordinates.Sum() == 0 ? TheBind : "Uncertainty Awaits";
		KeyView.text = Coordinates.Sum() == 0 ? "" : "1";
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
		Traveller.color = new Color32(255,255,255,255);
		while (Digger.isPlaying)
		{
			Traveller.text = TheBind[UnityEngine.Random.Range(0,TheBind.Length)].ToString() + TheBind[UnityEngine.Random.Range(0,TheBind.Length)].ToString() + "  " + TheBind[UnityEngine.Random.Range(0,TheBind.Length)].ToString() + TheBind[UnityEngine.Random.Range(0,TheBind.Length)].ToString() + "\n" + TheBind[UnityEngine.Random.Range(0,TheBind.Length)].ToString() + TheBind[UnityEngine.Random.Range(0,TheBind.Length)].ToString() + "  " + TheBind[UnityEngine.Random.Range(0,TheBind.Length)].ToString() + TheBind[UnityEngine.Random.Range(0,TheBind.Length)].ToString();
			Traveller.color = Color.Lerp(new Color32(255,255,255,255), new Color32(0,0,0,255), Digger.time / Digger.clip.length);
			Border.material.color = Color.Lerp(OldColor, new Color32(58,44,9,255), Digger.time / Digger.clip.length);
			yield return new WaitForSecondsRealtime(0.05f);
		}
		Traveller.text = "";
		Audio.PlaySoundAtTransform(SFX[13].name, transform);
		Border.material.color = new Color32(58,44,9,255);
		Module.HandlePass();
		ModuleSolved = true;
		yield return new WaitForSecondsRealtime(1f);
		Digger.clip = SFX[14];
		Digger.Play();
        Audio.PlaySoundAtTransform(SFX[15].name, transform);
		
		while (Digger.isPlaying)
		{
			Background.material.color = Color.Lerp(new Color32(0,0,0,255), new Color32(255,255,255,255), Digger.time / Digger.clip.length);
			yield return null;
		}
		Background.material.color = new Color32(255,255,255,255);
	}
	
	//twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"To move in the maze, use the command !{0} move [1 Letter Directions (Can be multiple: !{0} move UDLRTBFPOCHEAS)] | To check the current coordinates, use the command !{0} check | To press the top left button, use the command !{0} see/shut/redo | To toggle a dimensional coordinate during map view, use the command !{0} toggle [1-4 (in reading order)] (This command can be chained.)";
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
			
			string MovementAllowed = "UDLRTBFPOCHEAS";
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
			
			string NumbersAllowed = "1234";
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
