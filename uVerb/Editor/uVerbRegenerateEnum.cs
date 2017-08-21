using UnityEngine;
using UnityEditor;

namespace uVerb
{
    /**
     * uVerbRegenerateEnum : Regenerate Enum Used for Material Allocation
     * ==================================================================
     *  Editor Window Class
     * 
     *      PRIVATE
     *      =======
     *      generator   :  Instance of the Regenerator  
     */
    public class uVerbRegenerateEnum : EditorWindow
    {
        static uVerbEnumGenerator generator;

        /**
         * Init : Initialise Editor Window
         */
        [MenuItem("Window/uVerb/Regenerate Enum")]
        static void Init()
        {
            if (generator == null) generator = new uVerbEnumGenerator();
            uVerbRegenerateEnum window = (uVerbRegenerateEnum)GetWindow(typeof(uVerbRegenerateEnum));
            window.Show();
        }

        /**
         * OnGUI : Draw GUI for Editor Window
         */
        void OnGUI()
        {
            if (GUILayout.Button("Regenerate Enum"))
            {
                generator.GenerateClass();
            }
        }
    }
}
