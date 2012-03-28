using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using Engine;
namespace U5Designs {

    public class ZookeeperAI {
        Crate bossobject;
        public ZookeeperAI(Player player) {
            bossobject = new Crate(player);
            //initlize arrays of crates + boss(fallingbox)
            // the boss will be swapped around into diffrent locations in the array.
        }
        public void update(double time, PlayState playstate, Vector3 playerposn, bool enable3d) {
            //do Ai code, update the array of crates
            
            //tmp: pass update to the just the lone boss object for now
            bossobject.update(time, playstate, playerposn, enable3d);
        }
    }

    //the boss and the crates must both conform to this update
    internal interface fallingbox {
        void update(double time, PlayState playstate, Vector3 playerposn, bool enable3d);
    }

    public class Boss : GameObject, RenderObject, PhysicsObject, CombatObject, fallingbox {

        //see below this class for the crate object that isnt the boss (TODO)
        //physics vars
        internal Vector3 velocity;
        internal Vector3 accel;
        private bool doesGravity; //true if gravity affects this object

        public Player player;

        private bool pbox2d, pbox3d; //these controll if normal physics happens

        public Boss(Player player) {
            //TODO: set this up, make sure its right(especially animation).
            //physics stuff
            velocity = new Vector3(0, 0, 0);
            accel = new Vector3(0, 0, 0);
            doesGravity = false;
            _location = new Vector3(4200, 150, 50);
            _scale = new Vector3(12,12,12);
            _pbox = new Vector3(6,6,6);
            _cbox = new Vector3(6,6,6);
            _existsIn3d = true;
            _existsIn2d = true;
            pbox2d = true;
            pbox3d = true;

            //combat stuff
            _health = 20;
            _damage = 1;
            _speed = 1;
            _alive = true;
            _hascbox = true;
            //this should make the boss's sprite take damage like any enemy
            _type = 1;// type one means this is an enemy;

            //animation
            _mesh = mesh;
            _texture = texture;
            _sprite = null;
            _cycleNum = 0;
            _frameNum = 0;
            _is3dGeo = true;
            _animDirection = 1;
        }

        public void update(double time, PlayState playstate, Vector3 playerposn, bool enable3d) {


        }


        public void physUpdate3d(double time, List<PhysicsObject> physList) {
        }

        public void physUpdate2d(double time, List<PhysicsObject> physList) {
        }
        /*  The following are helper methods + getter/setters
       * 
       */
        //swaps physics box x and z coordinates (used for sprites that billboard)
        public void swapPBox() {
            float temp = _pbox.X;
            _pbox.X = _pbox.Z;
            _pbox.Z = temp;
        }

        //swaps combat box x and z coordinates (used for sprites that billboard)
        public void swapCBox() {
            float temp = _cbox.X;
            _cbox.X = _cbox.Z;
            _cbox.Z = temp;
        }
        private bool _is3dGeo;
        public bool is3dGeo {
            get { return _is3dGeo; }
        }

        private ObjMesh _mesh; //null for sprites
        public ObjMesh mesh {
            get { return _mesh; }
        }

        private MeshTexture _texture; //null for sprites
        public MeshTexture texture {
            get { return _texture; }
        }

        private SpriteSheet _sprite; //null for 3d objects
        public SpriteSheet sprite {
            get { return _sprite; }
        }

        private Vector3 _scale;
        public Vector3 scale {
            get { return _scale; }
        }

        private Vector3 _pbox;
        public Vector3 pbox {
            get { return _pbox; }
        }

        private Vector3 _cbox;
        public Vector3 cbox {
            get { return _cbox; }
        }

        private int _type;
        public int type {
            get { return _type; }
        }

        private int _cycleNum;
        public int cycleNumber {
            get { return _cycleNum; }
            set { _cycleNum = value; }
        }

        private double _frameNum; //index of the current animation frame
        public double frameNumber {
            get { return _frameNum; }
            set { _frameNum = value; }
        }

        public Billboarding billboards {
            get { return Billboarding.Yes; }
        }

        public bool collidesIn3d {
            get { return pbox3d; }
        }

        public bool collidesIn2d {
            get { return pbox2d; }
        }

        public void reset() {
            throw new NotImplementedException();
        }

        public void accelerate(Vector3 acceleration) {
            accel += acceleration;
        }

        private int _health;
        public int health {
            get { return _health; }
            set { _health = value; }
        }

        private int _damage;
        public int damage {
            get { return _damage; }
        }

        private float _speed;
        public float speed {
            get { return _speed; }
            set { _speed = value; }
        }

        private bool _alive;
        public bool alive {
            get { return _alive; }
            set { _alive = value; }
        }

        private int _animDirection;
        public int animDirection {
            get { return _animDirection; }
        }

        public int ScreenRegion {
            get { return screenRegion; }
        }

        public void doScaleTranslateAndTexture() {
            GL.PushMatrix();

            if (_is3dGeo) {
                _texture.doTexture();
            }

            GL.Translate(_location);
            GL.Scale(_scale);
        }
    }

    //this is the object consisting of the boss himself standing on a crate
    public class Crate : GameObject, RenderObject, PhysicsObject, CombatObject, fallingbox {
        //see below this class for the crate object that isnt the boss (TODO)
        //physics vars
        internal Vector3 velocity;
        internal Vector3 accel;
        private bool doesGravity; //true if gravity affects this object

        public Player player;

        private bool pbox2d, pbox3d; //these controll if normal physics happens

        public Crate(Player player) {
            //TODO: set this up, make sure its right(especially animation).
            //physics stuff
            velocity = new Vector3(0, 0, 0);
            accel = new Vector3(0, 0, 0);
            doesGravity = false;
            _location = new Vector3(4200, 150, 50);
            _scale = new Vector3(12,12,12);
            _pbox = new Vector3(6,6,6);
            _cbox = new Vector3(6,6,6);
            _existsIn3d = true;
            _existsIn2d = true;
            pbox2d = true;
            pbox3d = true;

            //combat stuff
            _health = 20;
            _damage = 1;
            _speed = 1;
            _alive = true;
            _hascbox = true;
            //this should make the boss's sprite take damage like any enemy
            _type = 1;// type one means this is an enemy;

            //animation
            _mesh = mesh;
            _texture = texture;
            _sprite = null;
            _cycleNum = 0;
            _frameNum = 0;
            _is3dGeo = true;
            _animDirection = 1;
        }

        public void update(double time, PlayState playstate, Vector3 playerposn, bool enable3d) {


        }


        public void physUpdate3d(double time, List<PhysicsObject> physList) {
        }

        public void physUpdate2d(double time, List<PhysicsObject> physList) {
        }
        /*  The following are helper methods + getter/setters
       * 
       */
        //swaps physics box x and z coordinates (used for sprites that billboard)
        public void swapPBox() {
            float temp = _pbox.X;
            _pbox.X = _pbox.Z;
            _pbox.Z = temp;
        }

        //swaps combat box x and z coordinates (used for sprites that billboard)
        public void swapCBox() {
            float temp = _cbox.X;
            _cbox.X = _cbox.Z;
            _cbox.Z = temp;
        }
        private bool _is3dGeo;
        public bool is3dGeo {
            get { return _is3dGeo; }
        }

        private ObjMesh _mesh; //null for sprites
        public ObjMesh mesh {
            get { return _mesh; }
        }

        private MeshTexture _texture; //null for sprites
        public MeshTexture texture {
            get { return _texture; }
        }

        private SpriteSheet _sprite; //null for 3d objects
        public SpriteSheet sprite {
            get { return _sprite; }
        }

        private Vector3 _scale;
        public Vector3 scale {
            get { return _scale; }
        }

        private Vector3 _pbox;
        public Vector3 pbox {
            get { return _pbox; }
        }

        private Vector3 _cbox;
        public Vector3 cbox {
            get { return _cbox; }
        }

        private int _type;
        public int type {
            get { return _type; }
        }

        private int _cycleNum;
        public int cycleNumber {
            get { return _cycleNum; }
            set { _cycleNum = value; }
        }

        private double _frameNum; //index of the current animation frame
        public double frameNumber {
            get { return _frameNum; }
            set { _frameNum = value; }
        }

        public Billboarding billboards {
            get { return Billboarding.Yes; }
        }

        public bool collidesIn3d {
            get { return pbox3d; }
        }

        public bool collidesIn2d {
            get { return pbox2d; }
        }

        public void reset() {
            throw new NotImplementedException();
        }

        public void accelerate(Vector3 acceleration) {
            accel += acceleration;
        }

        private int _health;
        public int health {
            get { return _health; }
            set { _health = value; }
        }

        private int _damage;
        public int damage {
            get { return _damage; }
        }

        private float _speed;
        public float speed {
            get { return _speed; }
            set { _speed = value; }
        }

        private bool _alive;
        public bool alive {
            get { return _alive; }
            set { _alive = value; }
        }

        private int _animDirection;
        public int animDirection {
            get { return _animDirection; }
        }

        public int ScreenRegion {
            get { return screenRegion; }
        }

        public void doScaleTranslateAndTexture() {
            GL.PushMatrix();

            if (_is3dGeo) {
                _texture.doTexture();
            }

            GL.Translate(_location);
            GL.Scale(_scale);
        }
    }
}
