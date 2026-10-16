using System;
using System.Collections.Generic;
namespace Melman {
public enum Difficulty { Easy, Normal, Hard }
public enum Phase { Playing, LevelComplete, GameOver, Won }
public struct Cell : IEquatable<Cell> {
 public int X, Y; public Cell(int x,int y){X=x;Y=y;}
 public bool Equals(Cell c){return X==c.X&&Y==c.Y;} public override bool Equals(object o){return o is Cell && Equals((Cell)o);} public override int GetHashCode(){return X*397^Y;}
 public static Cell operator +(Cell a,Cell b){return new Cell(a.X+b.X,a.Y+b.Y);}
}
public sealed class Game {
 public const int Size=19, LevelCount=5;
 public readonly HashSet<Cell> Walls=new HashSet<Cell>(), Dots=new HashSet<Cell>(), Powers=new HashSet<Cell>();
 public readonly List<Cell> Ghosts=new List<Cell>();
 public static readonly Cell[] Directions={new Cell(0,-1),new Cell(1,0),new Cell(0,1),new Cell(-1,0)};
 public Cell Player{get;private set;} public int Score{get;private set;} public int Lives{get;private set;} public int Level{get;private set;}
 public Difficulty Difficulty{get;private set;} public Phase Phase{get;private set;} public int PowerTicks{get;private set;}
 public float TickSeconds {get{return .21f-(int)Difficulty*.035f-(Level-1)*.008f;}}
 readonly Random random; int ticks; readonly Cell spawn=new Cell(1,1);
 public Game(Difficulty difficulty,int seed=42){Difficulty=difficulty;random=new Random(seed);Lives=3;Level=1;Build();}
 void Build(){
  Walls.Clear();Dots.Clear();Powers.Clear();Ghosts.Clear();
  for(int y=0;y<Size;y++)for(int x=0;x<Size;x++)Walls.Add(new Cell(x,y));
  var stack=new Stack<Cell>();var seen=new HashSet<Cell>();stack.Push(spawn);seen.Add(spawn);Walls.Remove(spawn);
  while(stack.Count>0){var p=stack.Peek();var options=new List<Cell>();foreach(var d in Directions){var n=p+d+d;if(n.X>0&&n.Y>0&&n.X<Size-1&&n.Y<Size-1&&!seen.Contains(n))options.Add(d);}
   if(options.Count==0){stack.Pop();continue;}var dir=options[random.Next(options.Count)];var next=p+dir+dir;Walls.Remove(p+dir);Walls.Remove(next);seen.Add(next);stack.Push(next);
  }
  // Add loops so ghosts can be evaded instead of forcing dead-end encounters.
  for(int y=1;y<Size-1;y++)for(int x=1;x<Size-1;x++){var c=new Cell(x,y);if(Walls.Contains(c)&&random.Next(5)==0&&((!Walls.Contains(new Cell(x-1,y))&&!Walls.Contains(new Cell(x+1,y)))||(!Walls.Contains(new Cell(x,y-1))&&!Walls.Contains(new Cell(x,y+1)))))Walls.Remove(c);}
  for(int y=1;y<Size-1;y++)for(int x=1;x<Size-1;x++){var c=new Cell(x,y);if(!Walls.Contains(c)&&!c.Equals(spawn))Dots.Add(c);}
  foreach(var c in new[]{new Cell(1,17),new Cell(17,1),new Cell(17,17)}){Powers.Add(c);Dots.Remove(c);}
  ResetActors();Phase=Phase.Playing;
 }
 void ResetActors(){Player=spawn;Ghosts.Clear();Ghosts.Add(new Cell(17,17));Ghosts.Add(new Cell(17,15));if(Difficulty!=Difficulty.Easy)Ghosts.Add(new Cell(15,17));PowerTicks=0;ticks=0;}
 public bool Walkable(Cell c){return c.X>=0&&c.Y>=0&&c.X<Size&&c.Y<Size&&!Walls.Contains(c);}
 public void Step(Cell direction){
  if(Phase!=Phase.Playing)return;
  ticks++;if(PowerTicks>0)PowerTicks--;
  if(Math.Abs(direction.X)+Math.Abs(direction.Y)==1&&Walkable(Player+direction))Player+=direction;
  if(Dots.Remove(Player))Score+=10;if(Powers.Remove(Player)){Score+=50;PowerTicks=35;}
  if(Collide())return;
  if(ticks%((Difficulty==Difficulty.Easy)?2:4)!=0||Difficulty==Difficulty.Hard){
   for(int i=0;i<Ghosts.Count;i++)Ghosts[i]=NextGhost(Ghosts[i],i);
  }
  if(Collide())return;
  if(Dots.Count+Powers.Count==0){Score+=Level*250;Phase=Level==LevelCount?Phase.Won:Phase.LevelComplete;}
 }
 bool Collide(){for(int i=0;i<Ghosts.Count;i++)if(Player.Equals(Ghosts[i])){if(PowerTicks>0){Score+=200;Ghosts[i]=Player.Equals(new Cell(17,17))?new Cell(17,1):new Cell(17,17);}else{Lives--;if(Lives<=0)Phase=Phase.GameOver;else ResetActors();return true;}}return false;}
 Cell NextGhost(Cell from,int index){
  var options=new List<Cell>();foreach(var d in Directions)if(Walkable(from+d))options.Add(from+d);
  if(PowerTicks>0){options.Sort((a,b)=>Distance(b,Player).CompareTo(Distance(a,Player)));return options[0];}
  if(random.Next(100)<(index==1?35:12))return options[random.Next(options.Count)];
  var queue=new Queue<Cell>();var first=new Dictionary<Cell,Cell>();queue.Enqueue(from);first[from]=from;
  while(queue.Count>0){var p=queue.Dequeue();foreach(var d in Directions){var n=p+d;if(!Walkable(n)||first.ContainsKey(n))continue;first[n]=p.Equals(from)?n:first[p];if(n.Equals(Player))return first[n];queue.Enqueue(n);}}
  return from;
 }
 static int Distance(Cell a,Cell b){return Math.Abs(a.X-b.X)+Math.Abs(a.Y-b.Y);}
 public void NextLevel(){if(Phase!=Phase.LevelComplete)return;Level++;Build();}
}
// Fixed simulation steps with a retained turn request and frame interpolation.
public sealed class Movement {
 readonly Game game;
 Cell direction, requested;
 bool started;
 float elapsed;
 public Cell PlayerFrom {get;private set;}
 public readonly List<Cell> GhostsFrom=new List<Cell>();
 public float Fraction {get{return Math.Min(1f,elapsed/game.TickSeconds);}}
 public Movement(Game game){this.game=game;Snapshot();}
 void Snapshot(){PlayerFrom=game.Player;GhostsFrom.Clear();GhostsFrom.AddRange(game.Ghosts);}
 public void Request(Cell value){if(Math.Abs(value.X)+Math.Abs(value.Y)==1)requested=value;}
 public void Advance(float seconds){
  if(seconds<0)throw new ArgumentOutOfRangeException(nameof(seconds));
  if(!started){
   if(!game.Walkable(game.Player+requested)||requested.Equals(new Cell()))return;
   started=true;Tick();
  }
  elapsed+=seconds;
  while(started&&elapsed>=game.TickSeconds&&game.Phase==Phase.Playing){elapsed-=game.TickSeconds;Tick();}
 }
 void Tick(){
  Snapshot();
  if(game.Walkable(game.Player+requested))direction=requested;
  int lives=game.Lives;game.Step(direction);
  if(game.Lives!=lives){Snapshot();direction=new Cell();requested=new Cell();started=false;elapsed=0;}
  for(int i=0;i<GhostsFrom.Count&&i<game.Ghosts.Count;i++)
   if(Math.Abs(GhostsFrom[i].X-game.Ghosts[i].X)+Math.Abs(GhostsFrom[i].Y-game.Ghosts[i].Y)>1)GhostsFrom[i]=game.Ghosts[i];
 }
}

public static class FirstPersonControls {
 // Grid Y grows south; positive quarter-turns rotate clockwise.
 public static Cell Direction(Cell facing,int turn){
  int turns=((turn%4)+4)%4;
  for(int i=0;i<turns;i++)facing=new Cell(-facing.Y,facing.X);
  return facing;
 }
}

}
