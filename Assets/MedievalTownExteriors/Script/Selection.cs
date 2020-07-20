using UnityEngine;
using System;

namespace RVI_TP05
{

    public class Selection : MonoBehaviour
    {

        #region Serialized Fields
        [SerializeField] private Material matTransparent;
        [SerializeField] private Camera camMain;
        [SerializeField] private Camera camCapture;
        [SerializeField] private Shader replace;
        #endregion


        #region Private Fields
        private MeshFilter _selectionWindowMeshFilter;
        private Plane _plane;
        private Vector3 posHG,posMouseHG,posBD,posMouseBD;
        private RenderTexture rt;
        private Texture2D capture;
        private float finalPos;
        GameObject theRectangle;
        #endregion

        /**
        Une partie du code est fournis par Alexandre HULSKEN
        Tous ce qui touche aux variables _selectionWindowMeshFilter et _plane est donnée par Alexandre
        Mon travail est au niveaux des fonctions de ChangeValue, ChangeDeepth, OnGUI et monveCamera
        Dans la fonction Update, j'ai pris 
        */
        #region Life Cycles
        private void Awake() {
            theRectangle = new GameObject("Selection Window");
            theRectangle.AddComponent<MeshRenderer>().material = matTransparent;

            _selectionWindowMeshFilter = theRectangle.AddComponent<MeshFilter>();
            _selectionWindowMeshFilter.mesh = new Mesh();
            _selectionWindowMeshFilter.mesh.vertices = new Vector3[4] {
                Vector3.zero,
                Vector3.zero,
                Vector3.zero,
                Vector3.zero
            };
            _selectionWindowMeshFilter.mesh.uv = new Vector2[4] {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };

            rt = new RenderTexture(camCapture.pixelWidth, camCapture.pixelHeight, 16);
            camCapture.targetTexture = rt;

            camMain.depthTextureMode = DepthTextureMode.Depth;
            camCapture.SetReplacementShader(replace, "");
            // //Debug.Log(camCapture.pixelWidth+" "+ camCapture.pixelHeight);

            camCapture.transform.position = camMain.transform.position;
            camCapture.transform.rotation = camMain.transform.rotation;

            capture = new Texture2D(camCapture.pixelWidth, camCapture.pixelHeight);

            posMouseHG = new Vector3(camCapture.pixelWidth,0,0);
            posMouseBD = new Vector3(0, camCapture.pixelHeight,0);

            finalPos = 0f;
        }

        public void ChangeValue() {
            int x0 = (int) posMouseHG.x; int y0 = (int) posMouseBD.y;
            int w = (int) Math.Abs(posMouseHG.x - posMouseBD.x); int h = (int) Math.Abs(posMouseBD.y - posMouseHG.y);
            // // Debug.Log(x0+ " " +y0+ " " +w+ " " +h+ " "+posMouseHG+" "+posMouseBD);

            Color[] c = capture.GetPixels (x0, y0, w, h);
            for (int x = 0; x < w; ++x) {
                for (int y = 0; y < h; ++y) {
                    c[y * w + x].g = 1.0f; // composante verte à 1
                }
            }

            capture.SetPixels(x0, y0, w, h,c); // affectation avec les couleurs modifiées
            capture.Apply(); // synchronisation
        }

        public float[] ChangeDeepth() {
            int x0 = (int) posMouseHG.x; int y0 = (int) posMouseBD.y;
            int w = (int) Math.Abs(posMouseHG.x - posMouseBD.x); int h = (int) Math.Abs(posMouseBD.y - posMouseHG.y);

            float max = -1;float min = 255;
            float[] res = new float[3];

            Color[] c = capture.GetPixels (x0, y0, w, h);
            for (int x = 0; x < w; ++x) {
                for (int y = 0; y < h; ++y) {
                    if (c[y * w + x].r > max) {
                        res[0]=x+x0;res[1]=y+y0;
                        max = c[y * w + x].r;
                    }

                    if (c[y * w + x].r < min ) {
                        min = c[y * w + x].r;
                    }
                }
            }

            res[2] = max;
            // c[((int) res[1]-y0) * w + ((int) res[0]+x0)].g = 1.0f;
            capture.SetPixels(x0, y0, w, h,c); // affectation avec les couleurs modifiées
            capture.Apply(); // synchronisation

            return res;
        }

        void OnGUI() {
            camCapture.transform.position = camMain.transform.position;
            camCapture.transform.rotation = camMain.transform.rotation;
            GUI.DrawTexture (new Rect (0, 0, 100,100), capture);
        }

        void moveCamera() {
            finalPos -= 0.1f;
            camMain.transform.Translate(0,0,0.1f);
        }

        private void Update() {
            if (finalPos>0) {
                 moveCamera();
            }

            RenderTexture save = RenderTexture.active; // pour ne pas polluer le moteur Unity
            Ray ray;

            RenderTexture.active = rt;
            capture.ReadPixels(new Rect (0, 0, camCapture.pixelWidth, camCapture.pixelHeight), 0, 0); // copier les pixels du render to texture dans la texture capture
            capture.Apply(); // synchronisation

            RenderTexture.active = save;

            if (Cursor.lockState == CursorLockMode.Locked) {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Mouse0)) {
                ray = camMain.ScreenPointToRay(Input.mousePosition);
                posMouseHG = Input.mousePosition;
                posHG = ray.origin + ray.direction*1;
            }

            if (Input.GetKeyUp(KeyCode.Mouse0)) {
            ray = camMain.ScreenPointToRay(Input.mousePosition);
            posMouseBD = Input.mousePosition;
            posBD = ray.origin + ray.direction*1;

            Vector3 posMIL = (posBD+posHG)/2;
            float[] close = ChangeDeepth();
            float near=1, far=10;
            float zndc = 1.0f-close[2]; // valeur de depth trouvée
            float depthZ = far*near/(far-(far-near)*zndc); // distance à la caméra

            // Debug.Log(depthZ+" "+posMIL);

            // Calcul du rectangle en fonction des position de la diagonal
            // Code fournis par Alexandre HULSKEN
            _plane = new Plane((-camMain.transform.forward).normalized, camMain.transform.position + camMain.transform.forward * depthZ);
            _selectionWindowMeshFilter.transform.position = posHG-transform.position;
            _selectionWindowMeshFilter.transform.rotation = camMain.transform.rotation;
            
            _selectionWindowMeshFilter.transform.position = Vector3.Lerp(transform.position + (posHG-transform.position) * depthZ, transform.position + (posBD-transform.position) * depthZ, .5f);
            Vector3 newPos = _selectionWindowMeshFilter.transform.InverseTransformPoint(transform.position + (posBD-transform.position) * depthZ);

            _selectionWindowMeshFilter.mesh.vertices = new Vector3[4] {
                new Vector3(-newPos.x, -newPos.y, 0),
                new Vector3(newPos.x, -newPos.y, 0),
                new Vector3(-newPos.x, newPos.y, 0),
                new Vector3(newPos.x, newPos.y, 0)
            };
            _selectionWindowMeshFilter.mesh.normals = new Vector3[4] {
                camMain.transform.forward,
                camMain.transform.forward,
                camMain.transform.forward,
                camMain.transform.forward
            };

            if (newPos.x * newPos.y < 0)
                _selectionWindowMeshFilter.mesh.triangles = new int[6] {
                    1, 3, 0, // lower left triangle
                    3, 2, 0  // upper right triangle
                };
            else
                _selectionWindowMeshFilter.mesh.triangles = new int[6] {
                    0, 2, 1, // lower left triangle
                    2, 3, 1  // upper right triangle
                };

            camMain.transform.LookAt(theRectangle.transform.position);
            // finalPos = ((posMIL-transform.position) * depthZ).z - 0.2f;
            finalPos = Vector3.Distance(theRectangle.transform.position,camMain.transform.position) - 0.5f;
            }
        }
        #endregion

    }

}