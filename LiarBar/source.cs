using System;
using UnityEngine;
using System.Reflection;
using MelonLoader;
using System.Collections.Generic;
using System.Linq;
public class UIHelper //https://www.unknowncheats.me/forum/unity/285864-beginners-guide-hacking-unity-games.html modified to fit needs
{
    private float
        x, y,
        width, height,
        margin,
        controlHeight,
        controlDist,
        nextControlY;

    public void Begin(string text, float _x, float _y, float _width, float _height, float _margin, float _controlHeight, float _controlDist)
    {
        x = _x;
        y = _y;
        width = _width;
        height = _height;
        margin = _margin;
        controlHeight = _controlHeight;
        controlDist = _controlDist;
        nextControlY = 20f + y;
        GUI.Box(new Rect(x, y, width, height), text);
    }

    private Rect NextControlRect()
    {
        Rect r = new Rect(x + margin, nextControlY, width - margin * 2, controlHeight);
        nextControlY += controlHeight + controlDist;
        return r;
    }

    public static string MakeEnable(string text, bool state)
    {
        return string.Format("{0}{1}", text, state ? "ON" : "OFF");
    }

    public bool Button(string text, bool state)
    {
        return Button(MakeEnable(text, state));
    }

    public bool Button(string text)
    {
        return GUI.Button(NextControlRect(), text);
    }
    public void RichLabel(string text)
    {
        GUIStyle style = new GUIStyle();
        style.richText = true;
        GUI.Label(NextControlRect(), text, style);
    }
    public void Label(string text, float value, int decimals = 2)
    {
        Label(string.Format("{0}{1}", text, Math.Round(value, 2).ToString()));
    }

    public void Label(string text)
    {
        GUI.Label(NextControlRect(), text);
    }

    public float Slider(float val, float min, float max)
    {
        return GUI.HorizontalSlider(NextControlRect(), val, min, max);
    }
}
public static class ReflectionExtensions
{
    public static T GetFieldValue<T>(this object obj, string name)  //https://stackoverflow.com/a/46488844
    {
        var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var field = obj.GetType().GetField(name, bindingFlags);
        return (T)field?.GetValue(obj);
    }
    public static void SetFieldValue<T>(this object obj, string name, T value)
    {
        var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var field = obj.GetType().GetField(name, bindingFlags);
        field.SetValue(obj, value);
        return;
    }
}
public static partial class EnumearbleExtensions
{
    public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int count)
    {
        if (null == source)
            throw new ArgumentNullException(nameof(source));
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        if (0 == count)
            yield break;

        // Optimization (see JonasH's comment)
        if (source is ICollection<T>)
        {
            foreach (T item in source.Skip(((ICollection<T>)source).Count - count))
                yield return item;

            yield break;
        }

        if (source is IReadOnlyCollection<T>)
        {
            foreach (T item in source.Skip(((IReadOnlyCollection<T>)source).Count - count))
                yield return item;

            yield break;
        }

        // General case, we have to enumerate source
        Queue<T> result = new Queue<T>();

        foreach (T item in source)
        {
            if (result.Count == count)
                result.Dequeue();

            result.Enqueue(item);
        }

        foreach (T item in result)
            yield return result.Dequeue();
    }
}
namespace LiarBar
{
    public class Hax : MelonMod
    {
        private Rect _boxPos = new Rect(0, 0, 300, 400);

        private bool _visible = false;
        public void Start()
        {
        }
        public override void OnUpdate()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Insert))
            {
                _visible = !_visible;

            }
        }
        enum Cards { Devil = -1, None = 0, King = 1, Queen = 2, Ace = 3, Joker = 4 }
        enum ChaosCards { None = 0, King = 1, Queen = 2, Chaos = 3, Master = 4 }
        public override void OnGUI()
        {
            if (_visible)
            {
                if (Event.current.type == EventType.MouseDrag && _boxPos.Contains(Event.current.mousePosition))
                {
                    _boxPos.position += Event.current.delta;
                }
                GUIStyle style = new GUIStyle();
                style.normal.background = Texture2D.grayTexture;

                GUILayout.BeginArea(new Rect( _boxPos.position.x, _boxPos.position.y, 300, 400),style);
                GUILayout.BeginVertical();
                if (Manager.Instance == null)
                {
                    GUILayout.Label("Not in game.");
                    GUILayout.EndVertical();
                    GUILayout.EndArea();

                    return;
                }
                
                if (Manager.Instance.GameStarted)
                {


                    if (Manager.Instance.mode == CustomNetworkManager.GameMode.LiarsDeck)
                    {
                        BlorfGamePlayManager gameManager = Manager.Instance.BlorfGame;
                        BlorfGamePlay localPlayer = Manager.Instance.GetLocalPlayer().GetComponent<BlorfGamePlay>();
                        if (localPlayer != null)
                        {
                            ReflectionExtensions.SetFieldValue<int>(localPlayer, "revolverbulllet", 5);                            
                        }
                        if(gameManager.DeckMode == BlorfGamePlayManager.deckmode.Deck2)
                        {
                            foreach (PlayerStats pS in Manager.Instance.Players)
                            {

                                if (pS.Dead || pS.Fnished)
                                    continue;
                                string text = pS.PlayerName + '\n';
                                foreach (GameObject cardObj in pS.GetComponent<BlorfGamePlay>().Cards)
                                {
                                    Card card = cardObj.GetComponent<Card>();
                                    int type = card.cardtype;
                                    bool devil = card.Devil;
                                    bool active = cardObj.activeSelf;
                                    if (active)
                                    {
                                        if (type == Manager.Instance.BlorfGame.RoundCard || type == (int)Cards.Joker)
                                        {
                                            text += " <color=green>" + ((Cards)type).ToString("G") + "</color>";
                                        }
                                        else
                                        {
                                            text += " " + ((Cards)type).ToString("G");
                                        }
                                    }
                                    if (devil)
                                        text += "(D)";
                                }
                                BlorfGamePlay player = pS.GetComponent<BlorfGamePlay>();
                                int current = ReflectionExtensions.GetFieldValue<int>(player, "currentrevoler");
                                int dies = ReflectionExtensions.GetFieldValue<int>(player, "revolverbulllet");
                                if (current == dies)
                                {
                                    text += "<color=red> B:" + current + "/" + (dies + 1) + "</color>";
                                }
                                else
                                {
                                    text += "<color=green> B:" + current + "/" + (dies + 1) + "</color>";
                                }
                                GUILayout.Label(text);
                            }
                            string text2 = "Table: ";
                            foreach (int card in Manager.Instance.BlorfGame.LastRound)
                            {
                                if (card == Manager.Instance.BlorfGame.RoundCard || card == (int)Cards.Joker)
                                {
                                    text2 += " <color=green>" + ((Cards)card).ToString() + "</color>";
                                }
                                else
                                {
                                    text2 += " <color=red>" + ((Cards)card).ToString() + "</color>";
                                }
                            }
                            GUILayout.Label(text2);
                            bool spotOn = true;
                            if (gameManager.LastRoundSpotOn.Count >= 4)
                            {
                                List<int> list = EnumearbleExtensions.TakeLast<int>(gameManager.LastRoundSpotOn, 4).ToList();
                                for(int i =0; i < list.Count; i++)
                                {
                                    if (list[i] != gameManager.RoundCard && list[i] != 4)
                                    {
                                        spotOn = false;
                                    }
                                }
                            }
                            else
                            {
                                spotOn = false;
                            }
                            string text3 = "Spot on: ";
                            if(spotOn)
                            {
                                text3 += "<color=green>Yes</color>";
                            }
                            else
                            {
                                text3 += "<color=red>No</color>";
                            }
                            GUILayout.Label(text3);
                                
                        }
                        if (gameManager.DeckMode == BlorfGamePlayManager.deckmode.Basic || gameManager.DeckMode == BlorfGamePlayManager.deckmode.Devil)
                        {
                            foreach (PlayerStats pS in Manager.Instance.Players)
                            {

                                if (pS.Dead || pS.Fnished)
                                    continue;
                                string text = pS.PlayerName + '\n';
                                foreach (GameObject cardObj in pS.GetComponent<BlorfGamePlay>().Cards)
                                {
                                    Card card = cardObj.GetComponent<Card>();
                                    int type = card.cardtype;
                                    bool devil = card.Devil;
                                    bool active = cardObj.activeSelf;
                                    if (active)
                                    {
                                        if (type == Manager.Instance.BlorfGame.RoundCard || type == (int)Cards.Joker)
                                        {
                                            text += " <color=green>" + ((Cards)type).ToString("G") + "</color>";
                                        }
                                        else
                                        {
                                            text += " " + ((Cards)type).ToString("G");
                                        }
                                    }
                                    else
                                    {
                                        text += " <color=red>" + ((Cards)type).ToString("G") + "</color>";

                                    }
                                    if (devil)
                                        text += "(D)";
                                }
                                BlorfGamePlay player = pS.GetComponent<BlorfGamePlay>();
                                int current = ReflectionExtensions.GetFieldValue<int>(player, "currentrevoler");
                                int dies = ReflectionExtensions.GetFieldValue<int>(player,"revolverbulllet");
                                if (current == dies)
                                {
                                    text += "<color=red> B:" + current + "/" + (dies + 1) + "</color>";
                                }
                                else
                                {
                                    text += "<color=green> B:" + current + "/" + (dies + 1) + "</color>";
                                }
                                GUILayout.Label(text);
                            }
                            string text2 = "Table: ";
                            foreach (int card in Manager.Instance.BlorfGame.LastRound)
                            {
                                if (card == Manager.Instance.BlorfGame.RoundCard || card == (int)Cards.Joker)
                                {
                                    text2 += " <color=green>" + ((Cards)card).ToString() + "</color>";
                                }
                                else
                                {
                                    text2 += " <color=red>" + ((Cards)card).ToString() + "</color>";
                                }
                            }
                            GUILayout.Label(text2);
                        }//end Liars Deck
                    }
                    else if (Manager.Instance.mode == CustomNetworkManager.GameMode.LiarsDice)
                    {
                        DiceGamePlayManager gameManager = Manager.Instance.DiceGame;
                        DiceGamePlay localPlayer = Manager.Instance.GetLocalPlayer().GetComponent<DiceGamePlay>();
                        if (localPlayer == null)
                        {
                            GUILayout.Label("Cant get local player.");
                            GUILayout.EndVertical();
                            GUILayout.EndArea();
                            return;
                        }
                        int sumOne = 0;
                        int sumTwo = 0;
                        int sumThree = 0;
                        int sumFour = 0;
                        int sumFive = 0;
                        int sumSix = 0;
                        string text = "";
                        foreach (PlayerStats pS in Manager.Instance.Players)
                        {
                            if (pS.Dead || pS.Fnished)
                                continue;
                            DiceGamePlay Player = pS.GetComponent<DiceGamePlay>();
                            text += pS.PlayerName + '\n';

                            foreach (int dice in Player.DiceValues)
                            {
                                text += dice.ToString() + ' ';
                                if (dice == 1)
                                {
                                    sumOne++;
                                    if (Manager.Instance.DiceGame.DiceMode == DiceGamePlayManager.dicemode.Traditional)
                                    {
                                        sumTwo++; sumThree++; sumFour++; sumFive++; sumSix++;
                                    }
                                }
                                else if (dice == 2)
                                { sumTwo++; }
                                else if (dice == 3)
                                { sumThree++; }
                                else if (dice == 4)
                                { sumFour++; }
                                else if (dice == 5)
                                { sumFive++; }
                                else if (dice == 6)
                                { sumSix++; }
                            }
                            text += '\n';
                        }
                        GUILayout.Label($"Sums: \nOnes: {sumOne} Twos: {sumTwo} Threes: {sumThree}\n Fours: {sumFour} Fives: {sumFive} Sixes: {sumSix}");
                        GUILayout.Label(text);
                    }
                    else if (Manager.Instance.mode == CustomNetworkManager.GameMode.LiarsChaos)
                    {
                        ChaosGamePlay localPlayer = Manager.Instance.GetLocalPlayer().GetComponent<ChaosGamePlay>();
                        if (localPlayer == null)
                        {
                            GUILayout.Label("Cant get local player.");
                            GUILayout.EndVertical();
                            GUILayout.EndArea();
                            return;
                        }
                        foreach (PlayerStats pS in Manager.Instance.Players)
                        {
                            if (pS.Dead || pS.Fnished)
                                continue;
                            string text = pS.PlayerName + '\n';
                            foreach (GameObject cardObj in pS.GetComponent<ChaosGamePlay>().Cards)
                            {
                                Card card = cardObj.GetComponent<Card>();
                                int type = card.cardtype;
                                bool active = cardObj.activeSelf;
                                if (active)
                                {
                                    if (type == Manager.Instance.ChaosGame.RoundCard || type == (int)ChaosCards.Master || type == (int)ChaosCards.Chaos)
                                    {
                                        text += " <color=green>" + ((ChaosCards)type).ToString("G") + "</color>";
                                    }
                                    else
                                    {
                                        text += " " + ((ChaosCards)type).ToString("G");
                                    }
                                }
                                else
                                {
                                    text += " <color=red>" + ((ChaosCards)type).ToString("G") + "</color>";

                                }

                            }
                            ChaosGamePlay player = pS.GetComponent<ChaosGamePlay>();
                            int current = ReflectionExtensions.GetFieldValue<int>(player,"currentrevoler");
                            int dies = ReflectionExtensions.GetFieldValue<int>(player,"revolverbulllet");
                            if (current == dies)
                            {
                                text += "<color=red> B:" + current + "/" + (dies + 1) + "</color>";
                            }
                            else
                            {
                                text += "<color=green> B:" + current + "/" + (dies + 1) + "</color>";
                            }
                            GUILayout.Label(text);
                        }
                        string text2 = "Table: ";
                        foreach (int card in Manager.Instance.ChaosGame.LastRound)
                        {
                            if (card == Manager.Instance.ChaosGame.RoundCard || card == (int)ChaosCards.Chaos || card == (int)ChaosCards.Master)
                            {
                                text2 += " <color=green>" + ((ChaosCards)card).ToString() + "</color>";
                            }
                            else
                            {
                                text2 += " <color=red>" + ((ChaosCards)card).ToString() + "</color>";
                            }
                        }
                        GUILayout.Label(text2);
                    }
                    else
                    {
                        GUILayout.Label("Unsupported gamemode.");
                        GUILayout.EndVertical();
                        GUILayout.EndArea();
                    }
                }
                else
                {
                    GUILayout.Label("Game not started.");

                }
                GUILayout.EndVertical();
                GUILayout.EndArea();
                if (Event.current.isMouse)
                    Event.current.Use();
            }
        }
    }


}
