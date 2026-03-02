//
//  request.swift
//  queue
//
//  Created by Pablo Rodriguez on 26/2/26.
//
import Foundation

struct EnqueueRequest: Codable {
    var destinationUrl: String
    var headers: [String: String]?
    var payload: String?
    var clientMessageId: String?
    var tenantId: String?
    var callbackUrl: String?
    var deliverAtUtc: Date?
}
