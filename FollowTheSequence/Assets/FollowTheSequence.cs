using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class FollowTheSequence : MonoBehaviour {

  public KMBombInfo Bomb;
  public KMAudio Audio;
  public GameObject[] Hatch;
  public KMSelectable Button;
  public KMSelectable WireSelectable;
  public Material[] Colors;
  public GameObject[] Wire;
  public GameObject[] LEDsGame;

  static int moduleIdCounter = 1;
  int moduleId;
  private bool moduleSolved;

	int[][] ColorOrder = new int[8][] { //ROYGBKWA
		new int[7] {4, 2, 1, 3, 6, 5, 7},
		new int[7] {7, 0, 3, 6, 5, 2, 4},
		new int[7] {1, 7, 4, 5, 3, 6, 0},
		new int[7] {2, 4, 5, 7, 1, 0, 6},
		new int[7] {0, 1, 6, 2, 7, 3, 5},
		new int[7] {3, 6, 7, 1, 0, 4, 2},
		new int[7] {5, 3, 2, 0, 4, 7, 1},
		new int[7] {6, 5, 0, 4, 3, 1, 3}
	};
  int[] StreakNumbers = {37, 45, 52, 60, 67, 75, 82, 98};
	int CurrentColor;
	int PreviousColor;
  int Iteration = -1;
  int Chances;
  int Streak;

  string[] ColorNames = {"red", "orange", "yellow", "green", "blue", "black", "white", "grey"};

  bool[][] LEDs = new bool[6][] {
      new bool[7], new bool[7], new bool[7], new bool[7], new bool[7], new bool[7]
  };
  bool MoveHatch;
  bool Open;
  bool ILikeYaCutG;
  bool Wrong;
  bool Animating;

  void Awake () {
      Wire[1].gameObject.SetActive(false);
      moduleId = moduleIdCounter++;
      Button.OnInteract += delegate () { ButtonPress(); return false; };
      WireSelectable.OnInteract += delegate () { WireCut(); return false; };
  }

  void Start () {
    WireColorSetter();
    Debug.LogFormat("[Follow The Sequence #{0}] At iteration {1}, the color is {2}. This is the first stage though. Cut it.", moduleId, Iteration + 2, ColorNames[CurrentColor]);
    PreviousColor = CurrentColor;
  }

  void WireCut () {
    ILikeYaCutG = true;
    Streak = 0;
    Wire[0].gameObject.SetActive(false);
    Wire[1].gameObject.SetActive(true);
  }

  void WireColorSetter () {
    if (Iteration == -1)
      goto FirstStageSkip;
    if (ILikeYaCutG)
      PreviousColor = CurrentColor;
    CurrentColor = UnityEngine.Random.Range(0, 100) <= StreakNumbers[Streak] ? ColorOrder[PreviousColor][Iteration] : UnityEngine.Random.Range(0, 8);
    FirstStageSkip:
    Wire[0].GetComponent<MeshRenderer>().material = Colors[CurrentColor];
    Wire[1].GetComponent<MeshRenderer>().material = Colors[CurrentColor];
  }

  IEnumerator HatchAnimation () {
      for (int j = 0; j < 10; j++)
      {
          if (Open)
          {
              Hatch[0].transform.localEulerAngles += new Vector3(0, 0, 4);
              Hatch[1].transform.localEulerAngles -= new Vector3(0, 0, 4);
          }
          else
          {
              Hatch[0].transform.localEulerAngles -= new Vector3(0, 0, 4);
              Hatch[1].transform.localEulerAngles += new Vector3(0, 0, 4);
          }
          yield return new WaitForSeconds(0.1f);
      }
      Open = !Open;
  }

  void ButtonPress () {
    if (Animating || moduleSolved)
      return;
    if (Iteration == -1) {
      GetComponent<KMBombModule>().HandleStrike();
      return;
    }
    Animating = true;
    if (Iteration == -1 && ILikeYaCutG)
      StartCoroutine(StagePass());
    else if (ColorOrder[PreviousColor][Iteration] == CurrentColor && ILikeYaCutG)
      StartCoroutine(StagePass());
    else if (ColorOrder[PreviousColor][Iteration] != CurrentColor && !ILikeYaCutG)
      StartCoroutine(StagePass());
    else {
      Wrong = true;
      StartCoroutine(StagePass());
    }
  }

  IEnumerator StagePass () {
    if (Iteration == 6) {
      GetComponent<KMBombModule>().HandlePass();
      moduleSolved = true;
      yield return null;
    }
    if (Wrong) {
      GetComponent<KMBombModule>().HandleStrike();
      Iteration = 0;
    }
    else
      Iteration++;
    StartCoroutine(HatchAnimation());
    yield return new WaitForSecondsRealtime(2f);
    if (!moduleSolved) {
      WireColorSetter();
      Wire[0].gameObject.SetActive(true);
      Wire[1].gameObject.SetActive(false);
      StartCoroutine(HatchAnimation());
      Debug.LogFormat("[Follow The Sequence #{0}] At iteration {1}, the color is {2}.", moduleId, Iteration + 2, ColorNames[CurrentColor]);
      if (ColorOrder[PreviousColor][Iteration] == CurrentColor)
        Debug.LogFormat("[Follow The Sequence #{0}] Cut it.", moduleId);
      else
        Debug.LogFormat("[Follow The Sequence #{0}] Don't cut it.", moduleId);
      yield return new WaitForSecondsRealtime(2f);
    }
    if (!ILikeYaCutG && !Wrong) {
      Streak++;
      Streak %= 8; //In case some mumbo jumbo shit happens
    }
    Debug.Log("Odds of cutting are " + StreakNumbers[Streak]);
    Wrong = false;
    ILikeYaCutG = false;
    Animating = false;
  }

  bool[] ShowingSegments (int Input) {
      bool[] Output = new bool[7];
      switch (Input) {    //tm tl    tr     mm     bl    br    bm
        case 0: Output = new bool[] {true, true, true, false, true, true, true}; break;
        case 1: Output = new bool[] {false, false, true, false, false, true, false}; break;
        case 2: Output = new bool[] {true, false, true, true, true, false, true}; break;
        case 3: Output = new bool[] {true, false, true, true, false, true, true}; break;
        case 4: Output = new bool[] {false, true, true, true, false, true, false}; break;
        case 5: Output = new bool[] {true, true, false, true, false, true, true}; break;
        case 6: Output = new bool[] {true, true, false, true, true, true, true}; break;
        case 7: Output = new bool[] {true, false, true, false, false, true, false}; break;
        case 8: Output = new bool[] {true, true, true, true, true, true, true}; break;
        case 9: Output = new bool[] {true, true, true, true, false, true, true}; break;
      }
      return Output;
    }

  void Update () {
    for (int i = 0; i < 7; i++)
      LEDsGame[i].gameObject.SetActive(ShowingSegments(Iteration + 2)[i]);
  }

  #pragma warning disable 414
  private readonly string TwitchHelpMessage = @"Use !{0} Cut to cut the wire. Use !{0} Next to press the button.";
  #pragma warning restore 414

  IEnumerator ProcessTwitchCommand (string Command) {
    yield return null;
    Command = Command.ToUpper();
    if (Command == "CUT")
      WireSelectable.OnInteract();
    else if (Command == "NEXT")
      Button.OnInteract();
    else
      yield return "sendtochaterror I don't understand!";
  }

  IEnumerator TwitchHandleForcedSolve () {
    while (!moduleSolved) {
      if (Iteration == -1) {
        yield return ProcessTwitchCommand("Cut");
        yield return ProcessTwitchCommand("Next");
        goto Weed;
      }
      else if (ColorOrder[PreviousColor][Iteration] == CurrentColor)
        yield return ProcessTwitchCommand("Cut");
      yield return ProcessTwitchCommand("Next");
      Weed:
      yield return true;
    }
  }
}
