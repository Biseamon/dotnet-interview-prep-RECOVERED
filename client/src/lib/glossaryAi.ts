import type { Term } from './glossary'

// AI / LLM engineering terminology, plain-English.
export const GLOSSARY_AI: Term[] = [
  // Core concepts
  { term: 'LLM', category: 'Core', definition: 'Large Language Model — a neural network trained on huge text that predicts the next token.' },
  { term: 'Token', category: 'Core', definition: 'The unit a model reads/writes — roughly ¾ of a word (~4 characters).' },
  { term: 'Context window', category: 'Core', definition: 'The maximum number of tokens a model can consider at once (prompt + response).' },
  { term: 'Prompt', category: 'Core', definition: 'The input text you give the model; prompt engineering shapes its behavior.' },
  { term: 'System prompt', category: 'Core', definition: 'A special instruction that sets the assistant\'s role and rules for the whole conversation.' },
  { term: 'Inference', category: 'Core', definition: 'Running a trained model to get outputs (vs training, which learns the weights).' },

  // Embeddings & retrieval
  { term: 'Embedding', category: 'Embeddings & RAG', definition: 'A vector of numbers representing text\'s meaning; similar meanings are close together.' },
  { term: 'Vector database', category: 'Embeddings & RAG', definition: 'Stores embeddings and finds the nearest ones fast (pgvector, Pinecone, Qdrant).' },
  { term: 'Cosine similarity', category: 'Embeddings & RAG', definition: 'Measures the angle between two vectors (1 = same direction, 0 = unrelated) — how relevance is scored.' },
  { term: 'RAG', category: 'Embeddings & RAG', definition: 'Retrieval-Augmented Generation — fetch relevant text and put it in the prompt so the model answers from your data.' },
  { term: 'Chunking', category: 'Embeddings & RAG', definition: 'Splitting documents into overlapping pieces to embed and retrieve.' },
  { term: 'Semantic search', category: 'Embeddings & RAG', definition: 'Search by meaning (via embeddings) rather than exact keywords.' },

  // Generation controls
  { term: 'Temperature', category: 'Generation', definition: 'Randomness of sampling: low = focused/deterministic, high = creative/varied.' },
  { term: 'Top-k / Top-p', category: 'Generation', definition: 'Limit sampling to the k most likely tokens, or the smallest set whose probability sums to p.' },
  { term: 'Logits', category: 'Generation', definition: 'Raw per-token scores the model outputs before turning them into probabilities.' },
  { term: 'Hallucination', category: 'Generation', definition: 'When a model confidently states something false — mitigate with RAG and verification.' },
  { term: 'Streaming', category: 'Generation', definition: 'Returning tokens as they\'re generated so users see output immediately.' },

  // Building blocks
  { term: 'Function / tool calling', category: 'Building blocks', definition: 'The model returns structured requests to call your functions (e.g. look up an order).' },
  { term: 'Agent', category: 'Building blocks', definition: 'An LLM that plans and takes multiple tool-using steps toward a goal.' },
  { term: 'Fine-tuning', category: 'Building blocks', definition: 'Further training a base model on your examples to specialize its behavior.' },
  { term: 'Prompt injection', category: 'Building blocks', definition: 'A security risk where malicious input hijacks the model\'s instructions — sanitize/untrust external text.' },
  { term: 'Guardrails', category: 'Building blocks', definition: 'Validation and policies around inputs/outputs (schemas, filters) to keep an AI app safe and correct.' },
  { term: 'Evaluation (evals)', category: 'Building blocks', definition: 'Measuring output quality with test sets, rubrics, or an LLM judge — how you know changes help.' },
]
