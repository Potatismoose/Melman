using System.Collections.Generic;
using UnityEngine;
namespace Melman {
public sealed class MelmanApp : MonoBehaviour {
 Game game; Difficulty difficulty=Difficulty.Normal; bool menu=true,paused,view3D; Movement movement; bool bestDirty;
 Camera cam; Transform board; readonly Dictionary<Cell,GameObject> pickups=new Dictionary<Cell,GameObject>();
 readonly List<Transform> ghosts=new List<Transform>(); Transform player; readonly List<Material> materials=new List<Material>();
 readonly Color ink=new Color(.035f,.055f,.12f), blue=new Color(.12f,.32f,.85f), mint=new Color(.2f,1,.75f), gold=new Color(1,.78f,.15f);
 GUIStyle title,text,button; int best;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot(){if(FindFirstObjectByType<MelmanApp>()==null)new GameObject("Melman").AddComponent<MelmanApp>();}
 void Awake(){
  best=PlayerPrefs.GetInt("Melman.Best",0);cam=Camera.main;if(cam==null)cam=new GameObject("Camera").AddComponent<Camera>();
  cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=ink;cam.tag="MainCamera";
  RenderSettings.ambientLight=Color.white*.75f;
  var light=new GameObject("Sun").AddComponent<Light>();light.type=LightType.Directional;light.intensity=.8f;light.transform.rotation=Quaternion.Euler(50,-30,0);
 }
 Material Mat(Color color){var m=new Material(Shader.Find("Standard"));m.color=color;materials.Add(m);return m;}
 Transform Shape(string label,PrimitiveType shape,Cell c,Vector3 scale,Material mat,float height){
  var o=GameObject.CreatePrimitive(shape);o.name=label;o.transform.SetParent(board);o.transform.position=World(c)+Vector3.up*height;o.transform.localScale=scale;o.GetComponent<Renderer>().sharedMaterial=mat;var collider=o.GetComponent<Collider>();if(collider)Destroy(collider);return o.transform;
 }
 Vector3 World(Cell c){return new Vector3(c.X-9,0,9-c.Y);}
 void NewGame(){game=new Game(difficulty,System.Environment.TickCount);menu=false;paused=false;Build();}
 void Build(){
  if(board)Destroy(board.gameObject);foreach(var m in materials)Destroy(m);materials.Clear();pickups.Clear();ghosts.Clear();board=new GameObject("Labyrinth").transform;
  var wall=Mat(blue);var dot=Mat(gold);var power=Mat(mint);var yellow=Mat(gold);var floor=Mat(new Color(.06f,.09f,.18f));
  Shape("Floor",PrimitiveType.Cube,new Cell(9,9),new Vector3(19,.15f,19),floor,-.25f);
  foreach(var c in game.Walls)Shape("Wall",PrimitiveType.Cube,c,new Vector3(.92f,.8f,.92f),wall,.3f);
  foreach(var c in game.Dots)pickups[c]=Shape("Spark",PrimitiveType.Sphere,c,Vector3.one*.16f,dot,.18f).gameObject;
  foreach(var c in game.Powers)pickups[c]=Shape("Power star",PrimitiveType.Sphere,c,Vector3.one*.4f,power,.25f).gameObject;
  player=Shape("Melman",PrimitiveType.Sphere,game.Player,Vector3.one*.72f,yellow,.45f);
  var eyes=Mat(ink);
  foreach(float x in new[]{-.15f,.15f}){var eye=Shape("Eye",PrimitiveType.Sphere,game.Player,Vector3.one*.12f,eyes,.8f);eye.SetParent(player);eye.localPosition=new Vector3(x,.38f,.12f);}
  Color[] colors={new Color(1,.3f,.45f),new Color(.6f,.4f,1),new Color(.2f,.85f,1)};
  for(int i=0;i<game.Ghosts.Count;i++)ghosts.Add(Shape("Ghost",PrimitiveType.Capsule,game.Ghosts[i],new Vector3(.6f,.45f,.6f),Mat(colors[i]),.45f));
  movement=new Movement(game);SetCamera();
 }
 void SetCamera(){if(cam==null)return;cam.orthographic=!view3D;cam.orthographicSize=12;cam.fieldOfView=52;cam.transform.position=view3D?new Vector3(0,19,-16):new Vector3(0,25,0);cam.transform.LookAt(Vector3.zero,Vector3.forward);if(view3D)cam.transform.LookAt(Vector3.zero,Vector3.up);}
 void Update(){
  if(Input.GetKeyDown(KeyCode.Tab)){view3D=!view3D;SetCamera();}
  if(menu||game==null)return;
  if(Input.GetKeyDown(KeyCode.Escape)||Input.GetKeyDown(KeyCode.P))paused=!paused;
  if(paused)return;
  // Capture key presses every frame, even between simulation steps. A turn
  // stays queued until its corridor opens; releasing the key does not lose it.
  if(Input.GetKeyDown(KeyCode.UpArrow)||Input.GetKeyDown(KeyCode.W))movement.Request(new Cell(0,-1));
  if(Input.GetKeyDown(KeyCode.DownArrow)||Input.GetKeyDown(KeyCode.S))movement.Request(new Cell(0,1));
  if(Input.GetKeyDown(KeyCode.LeftArrow)||Input.GetKeyDown(KeyCode.A))movement.Request(new Cell(-1,0));
  if(Input.GetKeyDown(KeyCode.RightArrow)||Input.GetKeyDown(KeyCode.D))movement.Request(new Cell(1,0));
  movement.Advance(Mathf.Min(Time.deltaTime,.25f));
  float fraction=movement.Fraction;
  player.position=Vector3.Lerp(World(movement.PlayerFrom),World(game.Player),fraction)+Vector3.up*.45f;
  foreach(var pair in pickups)if(pair.Value&&!game.Dots.Contains(pair.Key)&&!game.Powers.Contains(pair.Key))Destroy(pair.Value);
  for(int i=0;i<ghosts.Count;i++){
   ghosts[i].position=Vector3.Lerp(World(movement.GhostsFrom[i]),World(game.Ghosts[i]),fraction)+Vector3.up*.45f;
   ghosts[i].GetComponent<Renderer>().sharedMaterial.color=game.PowerTicks>0?mint:(i==0?new Color(1,.3f,.45f):i==1?new Color(.6f,.4f,1):new Color(.2f,.85f,1));
  }
  if(game.Score>best){best=game.Score;bestDirty=true;}
  if(game.Phase!=Phase.Playing)SaveBest();
 }
 void SaveBest(){if(!bestDirty)return;PlayerPrefs.SetInt("Melman.Best",best);PlayerPrefs.Save();bestDirty=false;}
 void OnApplicationPause(bool value){if(value)SaveBest();}
 void OnApplicationQuit(){SaveBest();}

 void Styles(){if(title!=null)return;title=new GUIStyle(GUI.skin.label){fontSize=54,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter};title.normal.textColor=gold;
  text=new GUIStyle(GUI.skin.label){fontSize=19,alignment=TextAnchor.MiddleCenter,wordWrap=true};text.normal.textColor=Color.white;
  button=new GUIStyle(GUI.skin.button){fontSize=20,fixedHeight=44};}
 void OnGUI(){Styles();float w=Screen.width,h=Screen.height;
  if(menu){Panel(new Rect(w/2-270,h/2-245,540,490));GUI.Label(new Rect(w/2-250,h/2-225,500,80),"MELMAN",title);GUI.Label(new Rect(w/2-240,h/2-143,480,60),"Till Melwin — ett stjärnäventyr i labyrinten",text);
   GUI.Label(new Rect(w/2-240,h/2-75,480,35),"REKORD  "+best,text);
   string[] names={"Lätt","Normal","Svår"};difficulty=(Difficulty)GUI.SelectionGrid(new Rect(w/2-230,h/2-25,460,44),(int)difficulty,names,3,button);
   if(GUI.Button(new Rect(w/2-170,h/2+40,340,50),"Spela",button))NewGame();
   GUI.Label(new Rect(w/2-230,h/2+110,460,100),"Piltangenter / WASD: flytta\nTab: 2D / 3D    •    P / Esc: paus\nSamla alla stjärnor. Gröna kraftkulor gör spöken sårbara.",text);return;}
  Panel(new Rect(0,0,w,65));GUI.Label(new Rect(10,12,w-20,40),"MELMAN   •   Bana "+game.Level+" / "+Game.LevelCount+"   •   Poäng "+game.Score+"   •   Liv "+game.Lives+"   •   Rekord "+best,text);
  if(GUI.Button(new Rect(15,h-52,145,40),view3D?"2D-vy [Tab]":"3D-vy [Tab]",button)){view3D=!view3D;SetCamera();}
  if(GUI.Button(new Rect(175,h-52,110,40),"Paus",button))paused=!paused;
  if(view3D)Minimap(w-210,80,190);
  if(game.PowerTicks>0)GUI.Label(new Rect(0,h-98,w,35),"STJÄRNKRAFT! "+game.PowerTicks,text);
  if(paused||game.Phase!=Phase.Playing){Panel(new Rect(w/2-240,h/2-140,480,280));string heading=paused?"Paus":game.Phase==Phase.LevelComplete?"Bana klar!":game.Phase==Phase.Won?"Du vann, stjärnspelare!":"Spelet är slut";
   GUI.Label(new Rect(w/2-220,h/2-115,440,55),heading,text);
   GUI.Label(new Rect(w/2-220,h/2-55,440,35),"Poäng: "+game.Score,text);
   if(GUI.Button(new Rect(w/2-170,h/2,340,45),paused?"Fortsätt":game.Phase==Phase.LevelComplete?"Nästa bana":"Spela igen",button)){if(paused)paused=false;else if(game.Phase==Phase.LevelComplete){game.NextLevel();Build();}else NewGame();}
   if(GUI.Button(new Rect(w/2-170,h/2+60,340,45),"Huvudmeny",button)){SaveBest();menu=true;paused=false;if(board)Destroy(board.gameObject);}
  }
 }
 void Panel(Rect r){var old=GUI.color;GUI.color=new Color(.025f,.04f,.09f,.96f);GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
 void Tile(Rect r,Color c){var old=GUI.color;GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
 void Minimap(float x,float y,float size){Panel(new Rect(x-5,y-5,size+10,size+10));float s=size/Game.Size;for(int row=0;row<Game.Size;row++)for(int col=0;col<Game.Size;col++){var c=new Cell(col,row);if(game.Walls.Contains(c))Tile(new Rect(x+col*s,y+row*s,s,s),blue);else if(game.Dots.Contains(c)||game.Powers.Contains(c))Tile(new Rect(x+col*s+s*.4f,y+row*s+s*.4f,s*.2f,s*.2f),Color.white);}
  foreach(var c in game.Ghosts)Tile(new Rect(x+c.X*s,y+c.Y*s,s,s),game.PowerTicks>0?mint:Color.magenta);Tile(new Rect(x+game.Player.X*s,y+game.Player.Y*s,s,s),gold);
 }
 void OnDestroy(){foreach(var m in materials)if(m)Destroy(m);}
}
}
