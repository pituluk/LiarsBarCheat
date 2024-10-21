using System;
using UnityEngine;
using System.Reflection;

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
    public static void SetFieldValue<T>(this object obj, string name,T value)
    {
        var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var field = obj.GetType().GetField(name, bindingFlags);
        field.SetValue(obj, value);
        return;
    }
}
namespace LiarBar
{
    public class Hax : MonoBehaviour
    {
        private Rect _boxPos = new Rect(0, 0, 300, 400);
        private UIHelper _helper = new UIHelper();
        private bool _visible = false;
        public void Start()
        {
        }
        public void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Insert))
            {
                _visible = !_visible;
            }
        }
        enum Cards {Devil = -1, None = 0, King = 1, Queen = 2, Ace = 3, Joker = 4 }
        public void OnGUI()
        {
            if (_visible)
            {
                if (Event.current.type == EventType.MouseDrag && _boxPos.Contains(Event.current.mousePosition))
                {
                    _boxPos.position += Event.current.delta;
                }
                _helper.Begin("Cheat", _boxPos.position.x, _boxPos.position.y, 300, 400, 0, 40, 30);
                if(System.Object.ReferenceEquals(null, Manager.Instance))
                {
                    _helper.Label("Not in game.");
                    return;
                }
                if (Manager.Instance.GameStarted)
                {
                    BlorfGamePlay localPlayer = Manager.Instance.GetLocalPlayer().GetComponent<BlorfGamePlay>();
                    int revolverBullet = localPlayer.GetFieldValue<int>("revolverbulllet");
                  //  if (revolverBullet <= 1)
                   // {
                        localPlayer.SetFieldValue<int>("revolverbulllet", 5); //either put it in that if to be more legit or just go last bullet always
                    //}
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
                                    text += " <color=green>" + ((Cards)type).ToString() + "</color>";
                                }
                                else
                                {
                                    text += " " + ((Cards)type).ToString();
                                }
                            }
                            else
                            {
                                text += " <color=red>" + ((Cards)type).ToString() + "</color>";
                                
                            }
                            if (devil)
                                text += "(D)";
                        }
                        BlorfGamePlay player = pS.GetComponent<BlorfGamePlay>();
                        int current = player.GetFieldValue<int>("currentrevoler");
                        int dies = player.GetFieldValue<int>("revolverbulllet");
                        if (current == dies) {
                            text += "<color=red> B:" + current + "/" + (dies+1) + "</color>";
                        }
                        else
                        {
                            text += "<color=green> B:" + current + "/" + (dies+1) + "</color>";
                        }
                        _helper.Label(text);
                    }
                    string text2 = "Table: ";
                    foreach(int card in Manager.Instance.BlorfGame.LastRound)
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
                    _helper.Label(text2);
                }
                else
                {
                    _helper.Label("Game not started.");
                }
                if (Event.current.isMouse)
                    Event.current.Use();
            }
        }
    }

    public class Loader
    {
        static UnityEngine.GameObject gameObject;

        public static void Load()
        {
            gameObject = new UnityEngine.GameObject();
            gameObject.AddComponent<Hax>();
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
        }

        public static void Unload()
        {
            UnityEngine.Object.Destroy(gameObject);
        }
    }
}
