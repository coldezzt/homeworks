import { useState } from 'react'
import { EchoRequest } from './generated/echo_pb'
import { EchoServiceClient } from './generated/EchoServiceClientPb'

export default function App() {
  const [message, setMessage] = useState('')
  const [response, setResponse] = useState('')

  const sendRequest = () => {
    const client = new EchoServiceClient('http://localhost:1400')
    const request = new EchoRequest()
    request.setMessage(message)
    
    client.echo(request, {}, (err, response) => {
      if (err) console.error(err);
      else {
        let msg = response.getMessage()
        console.log(msg)
        setResponse(msg)
      }
    })
  }

  return (
    <div>
      <input
        value={message}
        onChange={(e) => setMessage(e.target.value)}
        placeholder="Enter message"
      />
      <button onClick={sendRequest}>Send</button>
      <div>Response: {response}</div>
    </div>
  )
}