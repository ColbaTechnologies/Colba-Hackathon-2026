//
//  ContentView.swift
//  queue
//
//  Created by Pablo Rodriguez on 26/2/26.
//

import SwiftUI

struct ContentView: View {
    @Environment(\.openWindow) private var openWindow
    var body: some View {
        VStack {
            Spacer()
            Button("New Message") {
                openWindow(id: "send-form")
            }
           
            Spacer()

        }
        .padding()
    }
}

#Preview {
    ContentView()
}
