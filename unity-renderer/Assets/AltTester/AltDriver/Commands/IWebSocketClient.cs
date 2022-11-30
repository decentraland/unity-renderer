using System;
using AltWebSocketSharp;

namespace Altom.AltDriver.Commands
{
    public interface IWebSocketClient
    {
        //
        // Summary:
        //     Occurs when the WebSocketSharp.WebSocket gets an error.
        event EventHandler<ErrorEventArgs> OnError;
        //
        // Summary:
        //     Occurs when the WebSocketSharp.WebSocket receives a message.
        event EventHandler<string> OnMessage;

        //
        // Summary:
        // Occurs when the WebSocketSharp.WebSocket was closed
        //     
        event EventHandler<CloseEventArgs> OnClose;

        //
        // Summary:
        //     Sends text data using the WebSocket connection.
        //
        // Parameters:
        //   data:
        //     A System.String that represents the text data to send.
        void Send(string data);
        //
        // Summary:
        //     Closes the WebSocket connection, and releases all associated resources.
        void Close();
        bool IsAlive();
    }
}