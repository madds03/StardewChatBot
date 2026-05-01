from flask import Flask, request, jsonify
from openai import OpenAI
import os
from dotenv import load_dotenv

# Load API key from .env
load_dotenv()

app = Flask(__name__)

# Initialize OpenAI client
client = OpenAI(api_key=os.getenv("OPENAI_API_KEY"))

@app.route("/ask", methods=["POST"])
def ask():
    data = request.get_json()
    question = data.get("question", "").strip()

    if not question:
        return jsonify({"answer": "Please enter a question."})

    try:
        response = client.responses.create(
            model="gpt-4o-mini",
            input=question
        )

        answer = response.output_text

        return jsonify({"answer": answer})

    except Exception as e:
        return jsonify({"error": str(e)})

if __name__ == "__main__":
    print("Server running at http://127.0.0.1:8000")
    app.run(host="127.0.0.1", port=8000)