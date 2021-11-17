using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private InputController m_inputController;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_inputController.ReactToPress(KeyCode.Space);
        }
        
        if (Input.GetKeyUp(KeyCode.Space))
        {
            m_inputController.ReactToRelease(KeyCode.Space);
        }
        
        if (Input.GetKeyDown(KeyCode.LeftCommand))
        {
            m_inputController.ReactToPress(KeyCode.LeftCommand);
        }
        
        if (Input.GetKeyUp(KeyCode.LeftCommand))
        {
            m_inputController.ReactToRelease(KeyCode.LeftCommand);
        }
    }
}