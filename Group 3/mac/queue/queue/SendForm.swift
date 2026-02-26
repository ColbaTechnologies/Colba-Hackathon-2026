import SwiftUI

struct SendForm: View {
    
    @Environment(\.dismiss) private var dismiss
    
    @State private var destinationUrl = ""
    @State private var headersText = ""
    @State private var payload = ""
    @State private var clientMessageId = ""
    @State private var tenantId = ""
    @State private var callbackUrl = ""
    
    @State private var scheduleDelivery = false
    @State private var deliverAtUtc = Date()
    
    var body: some View {
        NavigationStack {
            Form {
                
                Section("Destination") {
                    TextField("Target URL", text: $destinationUrl)
                }
                
                Section("Optional Data") {
                    TextField("Payload (String)", text: $payload)
                    TextField("Client Message Id", text: $clientMessageId)
                    TextField("Tenant Id", text: $tenantId)
                    TextField("Callback URL", text: $callbackUrl)
                }
                
                Section("Headers (key:value per line)") {
                    TextEditor(text: $headersText)
                        .frame(height: 100)
                }
                
                Section {
                                    Toggle("Schedule delivery", isOn: $scheduleDelivery)
                                    
                                    if scheduleDelivery {
                                        DatePicker(
                                            "Deliver At",
                                            selection: $deliverAtUtc,
                                            displayedComponents: [.date, .hourAndMinute]
                                        )
                                    }
                                }
                
                Section {
                    Button("Send Request") {
                        sendEnqueueRequest(completion:  { _ in
                                print("Sent")
                        })
                    }
                    .disabled(destinationUrl.isEmpty)
                }
            }
            .navigationTitle("New Message")
            .toolbar {
                ToolbarItem {
                    Button("Cancel") {
                        dismiss()
                    }
                }
            }
        }
    }
    
    func sendEnqueueRequest( completion: @escaping (Result<Data, Error>) -> Void) {
        
        let headersDict = parseHeaders(headersText)
        
        let request = EnqueueRequest(
            destinationUrl: destinationUrl,
            headers: headersDict.isEmpty ? nil : headersDict,
            payload: payload.isEmpty ? nil : payload,
            clientMessageId: clientMessageId.isEmpty ? nil : clientMessageId,
            tenantId: tenantId.isEmpty ? nil : tenantId,
            callbackUrl: callbackUrl.isEmpty ? nil : callbackUrl,
            deliverAtUtc: scheduleDelivery ? deliverAtUtc : nil
        )
        
        // Convertimos el payload a JSON
        var urlRequest = URLRequest(url: URL(string: "https://webhooksite.net/b4e60ac0-d08a-4b15-ba29-bc5511c98748")!,timeoutInterval: Double.infinity)
        urlRequest.httpMethod = "POST"
        
        // Headers
        if let headers = request.headers {
            for (key, value) in headers {
                urlRequest.setValue(value, forHTTPHeaderField: key)
            }
        }
        
        
        // Envío
        URLSession.shared.dataTask(with: urlRequest) { data, response, error in
            if let error = error {
                completion(.failure(error))
            } else if let data = data {
                completion(.success(data))
            } else {
                completion(.failure(URLError(.unknown)))
            }
        }.resume()
    
        dismiss()
    }
    
    private func parseHeaders(_ text: String) -> [String: String] {
        var dict: [String: String] = [:]
        
        let lines = text.split(separator: "\n")
        for line in lines {
            let parts = line.split(separator: ":", maxSplits: 1)
            if parts.count == 2 {
                dict[String(parts[0]).trimmingCharacters(in: .whitespaces)] =
                String(parts[1]).trimmingCharacters(in: .whitespaces)
            }
        }
        
        return dict
    }
}
