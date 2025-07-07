// src/pages/AskPage.jsx
import React, { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../services/api';

export default function AskPage() {
  const nav = useNavigate();
  const [question, setQuestion] = useState('');
  const [messages, setMessages] = useState([]);  // {role:'user'|'bot', text:''}
  const [loading, setLoading] = useState(false);
  const scrollRef = useRef(null);

  /* Redirect if not logged in */
  useEffect(() => {
    const token = localStorage.getItem('token');
    if (!token) nav('/');
  }, [nav]);

  /* 🆕 Load chat history on mount */
  useEffect(() => {
    api.get('/chat/history')
      .then(res => {
        const history = res.data
          .reverse()   // oldest first for scroll flow
          .flatMap(h => ([
            { role:'user', text: h.question },
            { role:'bot',  text: h.answer  }
          ]));
        setMessages(history);
        scrollToBottom();
      })
      .catch(console.error);
  }, []);

  /* Scroll to bottom helper */
  const scrollToBottom = () => {
    setTimeout(() => {
      scrollRef.current?.scrollTo({ top: scrollRef.current.scrollHeight, behavior: 'smooth' });
    }, 100);
  };

  function formatParagraphs(text) {
            const sentences = text.match(/[^\.!\?]+[\.!\?]+/g) || [text];
            const chunks = [];
            let chunk = '';

            for (const sentence of sentences) {
            chunk += sentence.trim() + ' ';
            if (chunk.length > 250) {
                chunks.push(chunk.trim());
                chunk = '';
              }
             }
            if (chunk) chunks.push(chunk.trim());

            return chunks.join('\n\n');
  }
  
  const handleAsk = async (e) => {
    e.preventDefault();
    if (!question.trim()) return;

    const userMsg = { role:'user', text: question };
    setMessages(prev => [...prev, userMsg]);
    setQuestion('');
    setLoading(true);

    try {
      const { data } = await api.post('/chat/ask', { question });
      const botMsg = { role:'bot', text: formatParagraphs(data.answer) };
      setMessages(prev => [...prev, botMsg]);
    } catch {
      setMessages(prev => [...prev, { role:'bot', text:'❌ Error getting answer.' }]);
    } finally {
      setLoading(false);
      scrollToBottom();
    }
  };

  return (
    <div className="container mt-4" style={{ maxWidth: 720 }}>
      <h4 className="mb-4">Ask the Smart Assistant</h4>

      {/* Chat window */}
      <div ref={scrollRef}
           className="border rounded p-3 mb-3"
           style={{ height: 420, overflowY:'auto', background:'#f9f9f9' }}>
        {messages.length === 0 && (
          <p className="text-muted">Start by asking a question…</p>
        )}

        {messages.map((m,i) => (
          <div key={i} className={`d-flex ${m.role==='user' ? 'justify-content-end' : 'justify-content-start'} mb-2`}>
            <span className={`badge bg-${m.role==='user' ? 'primary' : 'secondary'}`}
             style={{ whiteSpace: 'pre-line', textAlign: 'left' }}
             >
              {m.text}
            </span>
          </div>
        ))}

        {loading && (
          <div className="text-center mt-2">
            <div className="spinner-border spinner-border-sm"/>
          </div>
        )}
      </div>

      {/* Input */}
      <form onSubmit={handleAsk}>
        <div className="input-group">
          <input className="form-control"
                 placeholder="Type your question…"
                 value={question}
                 onChange={e=>setQuestion(e.target.value)}
          />
          <button className="btn btn-success" type="submit" disabled={loading}>
            Ask
          </button>
        </div>
      </form>
    </div>
  );
}
