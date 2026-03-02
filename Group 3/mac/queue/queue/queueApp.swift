//
//  queueApp.swift
//  queue
//
//  Created by Pablo Rodriguez on 26/2/26.
//

import SwiftUI

@main
struct queueApp: App {
    var body: some Scene {
        WindowGroup {
            ContentView()
        }
        WindowGroup("Send Message", id: "send-form") {
                   SendForm().frame(minWidth: 500, minHeight: 500)
               }
        .defaultSize(width: 600, height: 500)
            .windowResizability(.contentSize)

    }
}
