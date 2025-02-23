**Grocery Expiration Tracker - Developer Specification**

## **Project Overview**

This project is a Blazor-based web application that allows users to upload photos of grocery receipts. Using OCR and AI, the app extracts product details, estimates expiration dates, and organizes food items into a calendar view. The goal is to help users manage their groceries efficiently, ensuring they consume items before they expire.

---

## **Core Features (Version 1.0)**

### **1. Receipt Processing Workflow**

- Users upload a receipt photo.
- Azure Computer Vision performs OCR to extract text.
- The extracted text is sent to an LLM (OpenAI API) for product matching.
- The LLM returns structured JSON with:
  - **Nutritional information** (calories, macronutrients, vitamins)
  - **Shelf life estimate**
  - **Food category**
  - **Unit & quantity**
- Confidence scores are assigned to each product match.
- If confidence is low, the user is prompted to:
  - Upload additional images of specific items.
  - Enter details manually.
- Receipt images are **not stored** after processing.

### **2. Expiration Calendar**

- The homepage displays a list of expiring foods in descending order.
- Dates are marked to the side, covering the current week.
- Past days are greyed out.
- Expiration status is determined in the UI based on stored dates.
- Bulk editing allows users to mark multiple items as "used," hiding them from the active list.

### **3. Shopping History**

- A separate page displays past purchases.
- Data is stored in a **NoSQL database** (likely Azure Cosmos DB).
- Purchases are grouped under each user rather than separate receipt documents.
- Stored fields per item:
  - Name
  - Expiration date
  - Nutritional info
  - Used status (boolean)
- Items older than **one week past expiration** are automatically cleared.
- Users can export their shopping history as JSON/CSV.

### **4. Authentication**

- OAuth integration (Microsoft, Google, etc.) to handle user authentication.
- No personal data (PII) is stored.

### **5. Logging & Error Handling**

- **Logging:** Integrated with **Azure Application Insights** for monitoring and debugging.
- **LLM Failures:** If an LLM request fails or returns insufficient data, the app falls back to user input.
- **API Rate Limits:** If an LLM rate limit is reached, users are prompted to try again later.

---

## **Technology Stack**

### **Frontend**

- Blazor Server for the UI.

### **Backend**

- **Rust** (preferred, but ASP.NET Core remains an alternative for better Semantic Kernel support).
- Azure Functions or an API service for handling receipt processing.
- OpenAI API for AI-powered product recognition.

### **Database & Storage**

- **Azure Cosmos DB** for storing user shopping history.
- **Azure Blob Storage (temporary use)** for receipt uploads before processing.

### **AI Services**

- **Azure Computer Vision** for OCR.
- **OpenAI API** for natural language processing and structured product extraction.

---

## **Testing Plan**

### **1. Unit Testing**

- **LLM Output Validation:** Ensure structured JSON output conforms to the expected schema.
- **OCR Processing:** Verify text extraction accuracy from receipts.
- **Database Operations:** Test user data storage and retrieval.

### **2. Integration Testing**

- **End-to-end receipt processing:** Simulate a receipt upload to final expiration tracking.
- **Authentication flow:** Ensure OAuth login works smoothly.
- **Bulk editing operations:** Confirm users can mark multiple items as "used."

### **3. Performance Testing**

- **LLM response times:** Measure processing speed and optimize for batch requests.
- **Database scaling:** Assess how well the NoSQL structure handles a large number of items.

### **4. UX Testing**

- **User Feedback Collection:** Conduct beta tests to refine UI interactions.
- **Accessibility Testing:** Ensure calendar views are readable and interactive.

---

## **Future Enhancements (Stretch Goals)**

- **Meal Planning:** Suggest meals based on grocery inventory.
- **Dietary Preferences:** Let users filter food tracking by dietary goals.
- **Advanced Analytics:** Use AI to analyze shopping patterns over time.
- **Mobile Camera Integration:** Allow direct receipt scanning from a mobile app.

---

## **Next Steps**

1. **Set up Azure resources** (Cosmos DB, Application Insights, Blob Storage, Computer Vision API).
2. **Develop receipt processing pipeline** (OCR -> LLM -> JSON extraction).
3. **Build Blazor frontend with OAuth authentication**.
4. **Implement expiration calendar and shopping history storage**.
5. **Integrate AI-powered insights for future versions.**

This specification provides a structured plan to begin development. Let me know if you'd like any modifications!


