
const MOCK_MODE = false;
const BASE_URL = 'http://localhost:5256';

const delay = (ms) => new Promise((r) => setTimeout(r, ms));

//  Token helpers 
export const getToken = () => localStorage.getItem('pkc_token');
export const setToken = (t) => localStorage.setItem('pkc_token', t);
export const clearToken = () => localStorage.removeItem('pkc_token');
export const getEmail = () => localStorage.getItem('pkc_email');
export const setEmail = (e) => localStorage.setItem('pkc_email', e);

const authHeaders = () => ({
  'Content-Type': 'application/json',
  Authorization: `Bearer ${getToken()}`,
});

//  Mock Data 
const MOCK_RESOURCES = [
  { id: '1', title: 'Attention Is All You Need', type: 'Url', status: 'Ready', sourceUrl: 'https://arxiv.org/abs/1706.03762', wordCount: 8200, createdAt: '2026-03-20T10:00:00Z' },
  { id: '2', title: 'Building RAG Pipelines with Semantic Kernel', type: 'Url', status: 'Ready', sourceUrl: 'https://devblogs.microsoft.com/semantic-kernel', wordCount: 3400, createdAt: '2026-03-22T14:30:00Z' },
  { id: '3', title: 'PostgreSQL pgvector Extension Guide', type: 'Url', status: 'Ready', sourceUrl: 'https://github.com/pgvector/pgvector', wordCount: 2100, createdAt: '2026-03-23T09:15:00Z' },
  { id: '4', title: 'My research notes on embeddings', type: 'Note', status: 'Ready', sourceUrl: null, wordCount: 540, createdAt: '2026-03-24T16:45:00Z' },
  { id: '5', title: 'ASP.NET Core 8 Background Services', type: 'Url', status: 'Embedding', sourceUrl: 'https://learn.microsoft.com/aspnet/core', wordCount: null, createdAt: '2026-03-27T11:00:00Z' },
  { id: '6', title: 'Vector Search Performance Benchmarks.pdf', type: 'Pdf', status: 'Chunking', sourceUrl: null, wordCount: null, createdAt: '2026-03-28T08:30:00Z' },
  { id: '7', title: 'ONNX Runtime in Production', type: 'Url', status: 'Failed', sourceUrl: 'https://onnxruntime.ai', wordCount: null, failureReason: 'Connection timeout', createdAt: '2026-03-25T13:00:00Z' },
];

const MOCK_NOTIFICATIONS = [
  { id: '1', type: 'resurface', unread: true, title: 'Related content resurfaced', description: 'You saved "Transformer Architecture Overview" 45 days ago - it\'s related to what you just added.', relatedResource: 'Transformer Architecture Overview', time: '2 minutes ago' },
  { id: '2', type: 'resurface', unread: true, title: 'Forgotten knowledge surfaced', description: '"BERT Pre-training Notes" from 2 months ago connects strongly to your recent pgvector resource.', relatedResource: 'BERT Pre-training Notes', time: '1 hour ago' },
  { id: '3', type: 'ready', unread: false, title: 'Resource processed successfully', description: '"Building RAG Pipelines with Semantic Kernel" is ready - embeddings generated, connections discovered.', relatedResource: 'Building RAG Pipelines with Semantic Kernel', time: '3 hours ago' },
  { id: '4', type: 'resurface', unread: false, title: 'Related content resurfaced', description: '"Cosine Similarity Explained" from last month is strongly connected to your new search results resource.', relatedResource: 'Cosine Similarity Explained', time: '1 day ago' },
  { id: '5', type: 'ready', unread: false, title: 'Resource processed successfully', description: '"PostgreSQL pgvector Extension Guide" is ready to query.', relatedResource: 'PostgreSQL pgvector Extension Guide', time: '2 days ago' },
];

//  Auth 
export async function apiRegister(email, password) {
  if (MOCK_MODE) {
    await delay(800);
    const token = 'mock.jwt.token.' + btoa(email);
    setToken(token);
    setEmail(email);
    return { token };
  }
  // REAL API:
  const res = await fetch(`${BASE_URL}/api/auth/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password }),
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error(err.message || 'Registration failed');
  }
  const data = await res.json();
  setToken(data.token);
  setEmail(email);
  return data;
}

export async function apiLogin(email, password) {
  if (MOCK_MODE) {
    await delay(700);
    if (email === 'demo@pkc.dev' && password === 'demo1234') {
      const token = 'mock.jwt.token.' + btoa(email);
      setToken(token);
      setEmail(email);
      return { token };
    }
    // Allow any credentials in mock mode for easy testing
    const token = 'mock.jwt.token.' + btoa(email);
    setToken(token);
    setEmail(email);
    return { token };
  }
  // REAL API:
  const res = await fetch(`${BASE_URL}/api/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password }),
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error(err.message || 'Invalid email or password');
  }
  const data = await res.json();
  setToken(data.token);
  setEmail(email);
  return data;
}

//  Resources 
export async function apiGetResources() {
  if (MOCK_MODE) {
    await delay(400);
    return [...MOCK_RESOURCES];
  }
  // REAL API:
  const res = await fetch(`${BASE_URL}/api/resources`, { headers: authHeaders() });
  if (!res.ok) throw new Error('Failed to fetch resources');
  return res.json();
}

export async function apiCreateUrl(url, title) {
  if (MOCK_MODE) {
    await delay(600);
    const newResource = {
      id: Date.now().toString(),
      title: title || url.replace(/^https?:\/\//, '').split('/')[0],
      type: 'Url',
      status: 'Pending',
      sourceUrl: url,
      wordCount: null,
      createdAt: new Date().toISOString(),
    };
    MOCK_RESOURCES.unshift(newResource);
    return { resourceId: newResource.id };
  }
  // REAL API:
  const res = await fetch(`${BASE_URL}/api/resources/url`, {
    method: 'POST',
    headers: authHeaders(),
    body: JSON.stringify({ url, title }),
  });
  if (!res.ok) throw new Error('Failed to save URL');
  return res.json();
}

export async function apiCreateNote(content, title) {
  if (MOCK_MODE) {
    await delay(500);
    const newResource = {
      id: Date.now().toString(),
      title: title || content.substring(0, 60) + '...',
      type: 'Note',
      status: 'Pending',
      sourceUrl: null,
      wordCount: content.split(' ').length,
      createdAt: new Date().toISOString(),
    };
    MOCK_RESOURCES.unshift(newResource);
    return { resourceId: newResource.id };
  }
  // REAL API:
  const res = await fetch(`${BASE_URL}/api/resources/note`, {
    method: 'POST',
    headers: authHeaders(),
    body: JSON.stringify({ content, title }),
  });
  if (!res.ok) throw new Error('Failed to save note');
  return res.json();
}

//  PDF Upload
export async function apiUploadPdf(file, title) {
  if (MOCK_MODE) {
    await delay(800);

    const newResource = {
      id: Date.now().toString(),
      title: title || file.name,
      type: 'Pdf',
      status: 'Pending',
      sourceUrl: null,
      wordCount: null,
      createdAt: new Date().toISOString(),
    };

    MOCK_RESOURCES.unshift(newResource);
    return { resourceId: newResource.id };
  }

  // REAL API:
  const formData = new FormData();
  formData.append('file', file);
  if (title) formData.append('title', title);

  const res = await fetch(`${BASE_URL}/api/resources/pdf`, {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${getToken()}`, // DO NOT add Content-Type
    },
    body: formData,
  });

  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error(err.message || 'Failed to upload PDF');
  }

  return res.json();
}

//  Search 
export async function apiSearch(query) {
  if (MOCK_MODE) {
    await delay(900);
    return [
      { id: '1a2b', content: 'The Transformer model architecture relies entirely on attention mechanisms, dispensing with recurrence and convolutions. The encoder maps an input sequence to a sequence of continuous representations.', score: 0.12 },
      { id: '2c3d', content: 'Multi-head attention allows the model to jointly attend to information from different representation subspaces at different positions, using h parallel attention layers.', score: 0.24 },
      { id: '3e4f', content: 'Positional encodings are added to input embeddings to inject information about the position of tokens in the sequence, using sine and cosine functions of different frequencies.', score: 0.31 },
    ];
  }
  // REAL API:
  const res = await fetch(`${BASE_URL}/api/search`, {
    method: 'POST',
    headers: authHeaders(),
    body: JSON.stringify({ query }),
  });
  if (!res.ok) throw new Error('Search failed');
  return res.json();
}

export async function apiAsk(query) {
  if (MOCK_MODE) {
    await delay(1400);
    return {
      answer: `Based on your saved content, the Transformer architecture uses self-attention mechanisms to process sequences in parallel. Unlike RNNs, it doesn't rely on sequential computation - instead, each token attends to all other tokens simultaneously. The "Attention is All You Need" paper introduced this in 2017, achieving state-of-the-art results on translation tasks with significantly less training time.`,
      related: [
        { content: 'Self-attention allows positions to attend to all positions in the previous layer.', score: 0.15 },
        { content: 'The model uses scaled dot-product attention: Attention(Q,K,V) = softmax(QKᵀ/√dk)V', score: 0.22 },
      ],
      topMatchId: '1a2b',
    };
  }
  // REAL API:
  const res = await fetch(`${BASE_URL}/api/ai/ask`, {
    method: 'POST',
    headers: authHeaders(),
    body: JSON.stringify({ query }),
  });
  if (!res.ok) throw new Error('AI query failed');
  return res.json();
}

//  Notifications (mock only - no backend endpoint yet) 
export async function apiGetNotifications() {
  await delay(300);
  return [...MOCK_NOTIFICATIONS];
}

export async function apiMarkNotificationRead(id) {
  await delay(200);
  const n = MOCK_NOTIFICATIONS.find((x) => x.id === id);
  if (n) n.unread = false;
  return { ok: true };
}
