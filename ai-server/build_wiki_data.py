import requests
import json
import time

PAGES = [
    "Crops",
    "Fish",
    "Villagers",
    "Bundles",
    "Community Center",
    "Spring",
    "Summer",
    "Fall",
    "Winter",
    "Leah",
    "Abigail",
    "Sebastian",
    "Shane",
    "Penny",
    "Haley",
    "Emily",
    "Alex",
    "Elliott",
    "Harvey",
    "Maru",
    "Sam"
]

API_URL = "https://stardewvalleywiki.com/mediawiki/api.php"

def get_page_text(title):
    params = {
        "action": "query",
        "prop": "extracts",
        "explaintext": True,
        "titles": title,
        "format": "json"
    }

    response = requests.get(API_URL, params=params, timeout=20)
    response.raise_for_status()

    data = response.json()
    pages = data["query"]["pages"]

    for page_id, page in pages.items():
        return page.get("extract", "")

    return ""

wiki_data = []

for title in PAGES:
    print(f"Downloading {title}...")
    text = get_page_text(title)

    if text.strip():
        wiki_data.append({
            "title": title,
            "content": text[:12000]
        })

    time.sleep(1)

with open("wiki_data.json", "w", encoding="utf-8") as f:
    json.dump(wiki_data, f, indent=2, ensure_ascii=False)

print(f"Saved {len(wiki_data)} pages to wiki_data.json")