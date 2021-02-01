using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System.Threading.Tasks;
using TrafficManager.Helpers;

namespace TrafficManager
{
    public class NodeEditor : BaseScript
    {
        private int[] nodes;

        private int scaleform;

        private bool isSkyEditor;

        private int camera;

        internal NodeEditor() { }

        public async void Enable(bool isSkyEditor)
        {
            this.isSkyEditor = isSkyEditor;

            if (isSkyEditor)
            {
                camera = CreateCam("DEFAULT_SCRIPTED_CAMERA", true);

                Vector3 myPos = GetEntityCoords(PlayerPedId(), true);
                SetCamCoord(camera, myPos.X, myPos.Y, myPos.Z + 100f);
                SetCamRot(camera, -90f, 0f, 0f, 2);
                SetCamDofFocalLengthMultiplier(camera, 0f);

                RenderScriptCams(true, true, 2500, true, false);
                nodes = new int[200];
                SetPlayerControl(PlayerId(), false, 0);
            }
            else
            {
                nodes = new int[40];
            }

            RegisterScript(this);
            await SetupScaleform();

            Tick += EditorThread;
            Tick += GetNodePositionsThread;
        }

        private async Task SetupScaleform()
        {
            scaleform = RequestScaleformMovie("instructional_buttons");
            while (!HasScaleformMovieLoaded(scaleform))
            {
                await Delay(0);
            }

            BeginScaleformMovieMethod(scaleform, "CLEAR_ALL");
            EndScaleformMovieMethod();

            BeginScaleformMovieMethod(scaleform, "SET_DATA_SLOT");
            ScaleformMovieMethodAddParamInt(0);
            PushButton(GetControlInstructionalButton(0, 25, 1));
            PushMessage("Exit editor");
            EndScaleformMovieMethod();

            if (isSkyEditor)
            {
                BeginScaleformMovieMethod(scaleform, "SET_DATA_SLOT");
                ScaleformMovieMethodAddParamInt(1);
                PushButton(GetControlInstructionalButton(0, 30, 1));
                PushButton(GetControlInstructionalButton(0, 33, 1));
                PushButton(GetControlInstructionalButton(0, 32, 1));
                PushMessage("Move Viewpoint");
                EndScaleformMovieMethod();

                BeginScaleformMovieMethod(scaleform, "SET_DATA_SLOT");
                ScaleformMovieMethodAddParamInt(2);
                PushButton(GetControlInstructionalButton(0, 24, 1));
                PushMessage("Toggle Node");
                EndScaleformMovieMethod();
            }
            else
            {
                BeginScaleformMovieMethod(scaleform, "SET_DATA_SLOT");
                ScaleformMovieMethodAddParamInt(1);
                PushButton(GetControlInstructionalButton(0, 38, 1));
                PushMessage("Toggle Node");
                EndScaleformMovieMethod();
            }

            BeginScaleformMovieMethod(scaleform, "DRAW_INSTRUCTIONAL_BUTTONS");
            EndScaleformMovieMethod();
        }

        private void PushButton(string button)
        {
            ScaleformMovieMethodAddParamPlayerNameString(button);
        }

        private void PushMessage(string message)
        {
            BeginTextCommandScaleformString("STRING");
            AddTextComponentSubstringKeyboardDisplay(message);
            EndTextCommandScaleformString();
        }

        private async Task EditorThread()
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                int nodeId = nodes[i];
                if (IsVehicleNodeIdValid(nodeId))
                {
                    Vector3 nodePos = new Vector3();
                    GetVehicleNodePosition(nodeId, ref nodePos);
                    if (GetVehicleNodeIsSwitchedOff(nodeId))
                    {
                        DrawMarker(28, nodePos.X, nodePos.Y, nodePos.Z, 0f, 0f, 0f, 0f, 0f, 0f, 1f, 1f, 1f, 255, 0, 0, 255, false, false, 2, false, null, null, false);
                    }
                    else
                    {
                        DrawMarker(28, nodePos.X, nodePos.Y, nodePos.Z, 0f, 0f, 0f, 0f, 0f, 0f, 1f, 1f, 1f, 0, 255, 0, 255, false, false, 2, false, null, null, false);
                    }
                }
            }


            DisableControlAction(0, 25, true);
            if (IsDisabledControlJustPressed(0, 25))
            {
                Disable();
                return;
            }

            DisableControlAction(0, isSkyEditor ? 24 : 38, true);
            if (IsDisabledControlJustPressed(0, isSkyEditor ? 24 : 38))
            {
                Vector3 hitPoint = new Vector3();
                if (isSkyEditor)
                {
                    float x = GetDisabledControlNormal(0, 239);
                    float y = GetDisabledControlNormal(0, 240);

                    int node = GetClosestNodeOnScreen(new Vector2(x, y));
                    if (node != 0)
                    {
                        GetVehicleNodePosition(node, ref hitPoint);
                    }
                }
                else
                {
                    hitPoint = GetEntityCoords(PlayerPedId(), true);
                }

                int nodeId = GetNthClosestVehicleNodeId(hitPoint.X, hitPoint.Y, hitPoint.Z, 0, 1, 0f, 0f);
                if (IsVehicleNodeIdValid(nodeId))
                {
                    Vector3 nodePos = new Vector3();
                    GetVehicleNodePosition(nodeId, ref nodePos);
                    if (Vector3.Distance(hitPoint, nodePos) < 2f)
                    {
                        bool enabled = GetVehicleNodeIsSwitchedOff(nodeId);
                        Node.Create(nodePos, enabled);
                    }
                }
            }

            if (isSkyEditor)
            {
                SetMouseCursorActiveThisFrame();
                SetMouseCursorSprite(1);

                Vector3 camPos = GetCamCoord(camera);

                // W
                DisableControlAction(0, 32, true);
                if (IsDisabledControlPressed(0, 32))
                {
                    camPos.Y += 1f;
                }

                // S
                DisableControlAction(0, 33, true);
                if (IsDisabledControlPressed(0, 33))
                {
                    camPos.Y -= 1f;
                }

                // A
                DisableControlAction(0, 30, true);
                if (IsDisabledControlPressed(0, 30))
                {
                    camPos.X += 1f;
                }

                // D
                DisableControlAction(0, 34, true);
                if (IsDisabledControlPressed(0, 34))
                {
                    camPos.X -= 1f;
                }

                SetCamCoord(camera, camPos.X, camPos.Y, camPos.Z);
            }

            DrawScaleformMovieFullscreen(scaleform, 255, 255, 255, 255, 0);

            await Task.FromResult(0);
        }

        private async Task GetNodePositionsThread()
        {
            Vector3 myPos = GetEntityCoords(PlayerPedId(), true);
            if (isSkyEditor)
            {
                Vector3 camPos = GetCamCoord(camera);
                myPos.X = camPos.X;
                myPos.Y = camPos.Y;
            }

            for (int i = 0; i < nodes.Length; i++)
            {
                int nodeId = GetNthClosestVehicleNodeId(myPos.X, myPos.Y, myPos.Z, i, 1, 0f, 0f);
                nodes[i] = nodeId;
                await Delay(0);
            }
        }

        private int GetClosestNodeOnScreen(Vector2 screen)
        {
            int closestNode = 0;
            float closestDistance = 0.2f;
            for (int i = 0; i < nodes.Length; i++)
            {
                int node = nodes[i];
                if (IsVehicleNodeIdValid(node))
                {
                    Vector3 nodeWorldPos = new Vector3();
                    GetVehicleNodePosition(node, ref nodeWorldPos);

                    Vector2 nodeScreenPos = new Vector2();
                    if (World3dToScreen2d(nodeWorldPos.X, nodeWorldPos.Y, nodeWorldPos.Z, ref nodeScreenPos.X, ref nodeScreenPos.Y))
                    {
                        float dist = Vector2.Distance(screen, nodeScreenPos);
                        if (dist < closestDistance)
                        {
                            closestNode = node;
                            closestDistance = dist;
                        }
                    }
                }
            }
            return closestNode;
        }

        public void Disable()
        {
            Tick -= GetNodePositionsThread;
            Tick -= EditorThread;

            SetScaleformMovieAsNoLongerNeeded(ref scaleform);
            UnregisterScript(this);

            if (isSkyEditor)
            {
                RenderScriptCams(false, true, 2500, false, false);
                DestroyCam(camera, true);
                SetPlayerControl(PlayerId(), true, 0);
            }
        }

        public static NodeEditor Instance { get; }

        static NodeEditor()
        {
            Instance = new NodeEditor();
        }
    }
}
