using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode.Components;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Unity.Services;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Core;
using Unity.Services.Authentication;
using TMPro;
using UnityEngine.SceneManagement;

public class MatchmakingManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _codeText;
    [SerializeField] private TMP_InputField _codeInput;
    [SerializeField] private GameObject _buttons;
    [SerializeField] private GameObject _failedText;
    [SerializeField] private GameObject _startCanvas;

    private UnityTransport _transport;
    private const int MaxPlayers = 4;

    private async void Awake()
    {
        _transport = FindObjectOfType<UnityTransport>();

        _buttons.SetActive(false);

        await Authenticate();

        _buttons.SetActive(true);
    }

    private static async Task Authenticate()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void HostNewGame()
    {
        _buttons.SetActive(false);

        Allocation a = await RelayService.Instance.CreateAllocationAsync(MaxPlayers);
        _codeText.text = "CODE: " + await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);
        _codeText.gameObject.SetActive(true);

        _transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);

        NetworkManager.Singleton.StartHost();

        _startCanvas.SetActive(true);
    }

    public async void JoinGame()
    {
        _buttons.SetActive(false);
        _failedText.SetActive(false);

        JoinAllocation a;

        try
        {
            a = await RelayService.Instance.JoinAllocationAsync((_codeInput.text).ToUpper());
        }
        catch
        {
            _buttons.SetActive(true);
            _failedText.SetActive(true);
            return;
        }

        _transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);

        NetworkManager.Singleton.StartClient();
    }

    
    public void StartGame()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("DOORS", LoadSceneMode.Single);
    }
}
